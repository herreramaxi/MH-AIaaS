using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Models;
using AIaaS.WebAPI.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

            return Ok(workflow);
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
    }
}
