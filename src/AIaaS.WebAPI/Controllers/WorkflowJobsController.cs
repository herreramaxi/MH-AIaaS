using AIaaS.Application.Features.WorkflowJobs.Queries.GetWorkflowJobs;
using AIaaS.Application.Features.Workflows.Queries.GetLatestWorkflowRunHistory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIaaS.WebAPI.Controllers
{
    [Authorize(Policy = "Administrator")]
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowJobsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WorkflowJobsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id?}")]
        public async Task<IActionResult> Get([FromRoute] int? id)
        {
            var workflowRunHistories = await _mediator.Send(new GetWorkflowJobsRequest(id));
            return Ok(workflowRunHistories);
        }

        [HttpGet("getLatestWorkflowRunHistory/{workflowId:int}")]
        public async Task<IActionResult> GetLatestWorkflowRunHistory(int workflowId)
        {
            var query = new GetLatestWorkflowRunHistoryQuery(workflowId);
            var workflowRunHistory = await _mediator.Send(query);

            return Ok(workflowRunHistory);
        }

        [HttpGet("getWorkflowJobDetails/{workflowRunId}")]
        public async Task<IActionResult> GetWorkflowJobDetails([FromRoute] int workflowRunId)
        {
            var workflowNodeRunHistories = await _mediator.Send(new GetWorkflowJobDetailsRequest(workflowRunId));
            return Ok(workflowNodeRunHistories);
        }

       
    }
}
