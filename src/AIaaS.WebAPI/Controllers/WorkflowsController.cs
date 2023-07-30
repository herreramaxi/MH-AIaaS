using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Application.Features.Workflows.Commands;
using AIaaS.Application.Features.Workflows.Commands.ValidateAndSaveWorkflow;
using AIaaS.Application.Features.Workflows.Queries;
using AIaaS.Application.Features.Workflows.Queries.GetLatestWorkflowRunHistory;
using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CleanArchitecture.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIaaS.WebAPI.Controllers
{
    [Authorize(Policy = "Administrator")]
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowsController : ControllerBase
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IMediator _mediator;

        public WorkflowsController(IApplicationDbContext dbContext, IMediator mediator)
        {
            _dbContext = dbContext;
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var query = new GetAllWorkflowsQuery();
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [HttpGet]
        [Route("{id:int:min(1)}")]
        public async Task<ActionResult<WorkflowDto>> GetById(int id)
        {
            var query = new GetWorkflowByIdQuery(id);
            var workflow = await _mediator.Send(query);

            return workflow.ToActionResult(this);
        }

        [HttpPost]
        public async Task<ActionResult<WorkflowDto>> Create([FromBody] bool useMLTemplate = false)
        {
            var query = new CreateWorkflowCommand(useMLTemplate);
            var result = await _mediator.Send(query);

            return result.IsSuccess ?
                CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value) :
                result.ToActionResult(this);
        }

        [HttpPut]
        public async Task<ActionResult<WorkflowDto>> Save(WorkflowDto workflowDto)
        {
            var command = new ValidateAndSaveCommand(workflowDto);
            var workflow = await _mediator.Send(command);

            return workflow.ToActionResult(this);
        }

        [HttpPost("rename")]
        public async Task<ActionResult<WorkflowDto>> Rename(WorkflowRenameParameter renameParameter)
        {
            var command = new RenameWorkflowCommand(renameParameter);
            var workflow = await _mediator.Send(command);

            return workflow.ToActionResult(this);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Remove([FromRoute] int id)
        {
            var command = new RemoveWorkflowCommand(id);
            var workflow = await _mediator.Send(command);

            return workflow.ToActionResult(this);
        }

        [HttpPost("run")]
        public async Task<ActionResult<WorkflowDto>> Run(WorkflowDto workflowDto)
        {
            var command = new RunWorkflowCommand(workflowDto);
            var workflow = await _mediator.Send(command);

            return workflow.ToActionResult(this);
        }

        [HttpGet("getPreview/{workflowDataviewId:int}")]
        public async Task<ActionResult<DataViewFilePreviewDto?>> GetPreview(int workflowDataviewId)
        {
            var query = new GetPreviewWorkflowQuery(workflowDataviewId);

            var previewResult = await _mediator.Send(query);

            return previewResult.ToActionResult(this);
        }
    }
}
