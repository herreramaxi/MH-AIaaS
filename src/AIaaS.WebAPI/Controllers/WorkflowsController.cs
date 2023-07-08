using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Interfaces;
using AIaaS.WebAPI.Models;
using AIaaS.WebAPI.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using System.Security.Claims;

namespace AIaaS.WebAPI.Controllers
{
    [Authorize(Policy = "Administrator")]
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowsController : ControllerBase
    {
        private readonly EfContext _dbContext;

        public WorkflowsController(EfContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var workflows = await _dbContext.Workflows.ToListAsync();

            return Ok(workflows);
        }

        [HttpGet]
        [Route("{id:int:min(1)}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest("Id parameter should be greater than zero");

            var workflow = await _dbContext.Workflows
                .Include(w => w.MLModel)
                .ThenInclude(m => m.Endpoint)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (workflow is null)
                return NotFound();

            var dto = new WorkflowDto()
            {
                Id = workflow.Id,
                Name = workflow.Name,
                Description = workflow.Description,
                IsPublished = workflow.MLModel?.Endpoint is not null,
                IsModelGenerated = workflow.MLModel is not null,
                CreatedBy = workflow.CreatedBy,
                CreatedOn = workflow.CreatedOn,
                ModifiedBy = workflow.ModifiedBy,
                ModifiedOn = workflow.ModifiedOn,
                Root = workflow.Data
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            var userEmail = this.User.FindFirst(ClaimTypes.Email)?.Value;
            if (userEmail == null)
                return BadRequest("User email not found on request");

            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email.ToLower().Equals(userEmail.ToLower()));
            if (user == null)
                return BadRequest("User not found");

            var workflow = new Workflow()
            {
                Name = $"Workflow-created ({DateTime.Now})",
                User = user
            };

            await _dbContext.Workflows.AddAsync(workflow);
            await _dbContext.SaveChangesAsync();

            var dto = new WorkflowDto
            {
                Name = workflow.Name,
                Id = workflow.Id,
                CreatedBy = user.CreatedBy,
                CreatedOn = user.CreatedOn
            };

            return CreatedAtAction(nameof(GetById), new { id = workflow.Id }, dto);
        }

        [HttpPut]
        public async Task<IActionResult> Save([FromServices] IWorkflowService workflowService, WorkflowDto workflowDto)
        {
            var serviceResult = await workflowService.Save(workflowDto);

            return serviceResult.Status switch
            {
                Ardalis.Result.ResultStatus.Ok => Ok(serviceResult.Value),
                Ardalis.Result.ResultStatus.NotFound => NotFound(serviceResult.Errors.FirstOrDefault()),
                Ardalis.Result.ResultStatus.Error => BadRequest(serviceResult.Errors.FirstOrDefault()),
                _ => StatusCode(500, serviceResult.Errors.FirstOrDefault() ?? "Unknown error"),
            };
        }


        [HttpPost("rename")]
        public async Task<IActionResult> Rename(WorkflowRenameParameter renameParameter)
        {
            if (renameParameter.Id <= 0)
                return BadRequest("id must be greater than zero");

            if (string.IsNullOrEmpty(renameParameter.Name))
                return BadRequest("name is required");

            var workflowFromDb = await _dbContext.Workflows.FindAsync(renameParameter.Id);

            if (workflowFromDb is null) return NotFound();

            workflowFromDb.Name = renameParameter.Name;
            workflowFromDb.Description = renameParameter.Description;

            _dbContext.Workflows.Update(workflowFromDb);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = renameParameter.Id }, workflowFromDb);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Remove([FromRoute] int id)
        {
            if (id <= 0)
                return BadRequest("Id parameter should be greater than zero");

            var workflow = await _dbContext.Workflows.FirstOrDefaultAsync(x => x.Id == id);
            if (workflow is null)
                return NotFound();

            _dbContext.Workflows.Remove(workflow);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("run")]
        public async Task<IActionResult> Run([FromServices] IWorkflowService workflowService, WorkflowDto workflowDto)
        {
            var serviceResult = await workflowService.Run(workflowDto);

            return serviceResult.Status switch
            {
                Ardalis.Result.ResultStatus.Ok => Ok(serviceResult.Value),
                Ardalis.Result.ResultStatus.NotFound => NotFound(serviceResult.Errors.FirstOrDefault()),
                Ardalis.Result.ResultStatus.Error => BadRequest(serviceResult.Errors.FirstOrDefault()),
                _ => StatusCode(500, serviceResult.Errors.FirstOrDefault() ?? "Unknown error"),
            };
        }

        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromServices] IWorkflowService workflowService, WorkflowDto workflowDto)
        {
            var serviceResult = await workflowService.Validate(workflowDto);

            return serviceResult.Status switch
            {
                Ardalis.Result.ResultStatus.Ok => Ok(serviceResult.Value),
                Ardalis.Result.ResultStatus.NotFound => NotFound(serviceResult.Errors.FirstOrDefault()),
                Ardalis.Result.ResultStatus.Error => BadRequest(serviceResult.Errors.FirstOrDefault()),
                _ => StatusCode(500, serviceResult.Errors.FirstOrDefault() ?? "Unknown error"),
            };
        }

        [HttpGet("GetPreview/{workflowDataviewId:int}")]
        public async Task<ActionResult> GetPreview(int workflowDataviewId)
        {
            var dataView = await _dbContext.WorkflowDataViews.FindAsync(workflowDataviewId);
            if (dataView?.Data is null) return NotFound();

            using var memStream = new MemoryStream(dataView.Data);
            var mss = new MultiStreamSourceFile(memStream);
            var mlContext = new MLContext();
            var dataview = mlContext.Data.LoadFromBinary(mss);
            var header = dataview.Schema.Select(x => x.Name);
            var MaxRows = 100;
            var preview = dataview.Preview(maxRows: MaxRows);

            var records = new List<string[]>();
            var columns = preview.Schema.Where(x => !x.IsHidden).Select(x => new { x.Index, x.Name });
            var columnIndices = columns.Select(x => x.Index).ToHashSet();

            foreach (var row in preview.RowView)
            {
                var record = row.Values
                    .Where((x, i) => columnIndices.Contains(i))
                    .Select(x => x.Value?.ToString() ?? "")
                    .ToArray();

                records.Add(record);
            }

            return Ok(new
            {
                header = header,
                rows = records
            });

        }
    }
}
