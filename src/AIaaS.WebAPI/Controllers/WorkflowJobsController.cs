using AIaaS.Application.Features.WorkflowJobs.Queries.GetWorkflowJobs;
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
    }
}
