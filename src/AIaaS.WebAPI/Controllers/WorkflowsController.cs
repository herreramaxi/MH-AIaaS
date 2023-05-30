using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Interfaces;
using AIaaS.WebAPI.Models;
using AIaaS.WebAPI.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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

            var workflow = await _dbContext.FindAsync<Workflow>(id);

            if (workflow is null)
                return NotFound();

            var dto = new WorkflowDto()
            {
                Id = workflow.Id,
                Name = workflow.Name,
                Description = workflow.Description,
                IsPublished = workflow.IsPublished,
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
            var workflow = new Workflow()
            {
                Name = $"Workflow-created ({DateTime.Now})"
            };

            await _dbContext.Workflows.AddAsync(workflow);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = workflow.Id }, workflow);
        }

        [HttpPut]
        public async Task<IActionResult> Save(WorkflowDto workflowDto)
        {
            if (workflowDto == null)
                return BadRequest("Workflow is required");

            if (workflowDto.Id <= 0)
                return BadRequest("Workflow id must be greater than zero");

            var workflow = await _dbContext.Workflows.FindAsync(workflowDto.Id);
            if (workflow == null)
                return NotFound();

            //var graphSerialized = JsonSerializer.Serialize(workflowDto.Root);
            workflow.Data = workflowDto.Root;

            _dbContext.Workflows.Update(workflow);
            await _dbContext.SaveChangesAsync();

            return NoContent();
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
        public async Task<IActionResult> Run([FromServices] IWorkflowService  workflowService, WorkflowDto workflowDto)
        {
            var workflow = await _dbContext.Workflows.FindAsync(workflowDto.Id);

            if (workflow is null) {
                return NotFound("Workflow not found");
            }

            if (string.IsNullOrEmpty(workflowDto?.Root))
                return BadRequest("Workflow is required");

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true};
            var workflowGraphDto = JsonSerializer.Deserialize<WorkflowGraphDto>(workflowDto.Root, jsonOptions);
           
            if (workflowGraphDto is null)
            {
                return BadRequest("Not able to process workflow");
            }

           await workflowService.Run(workflowGraphDto, workflow);

            //if (workflowGraphDto.Root.Type?.Equals("dataset", StringComparison.CurrentCultureIgnoreCase) != true)
            //    return BadRequest("Dataset node not found");

         
            return Ok();
        }

    }
}
