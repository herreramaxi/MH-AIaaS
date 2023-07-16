using AIaaS.Application.Features.Predictions.Queries.GetPredictionInputSample;
using AIaaS.Application.Features.Predictions.Queries.PredictInputSample;
using AIaaS.WebAPI.Infrastructure;
using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIaaS.WebAPI.Controllers
{
    [Authorize(Policy = "Administrator")]
    [Route("api/[controller]")]
    [ApiController]
    public class PredictController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PredictController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("getPredictionInputSample/{endpointId}")]
        public async Task<ActionResult<object?>> GetPredictionInputSample([FromRoute] int endpointId)
        {
            var result = await _mediator.Send(new GetPredictionInputSampleRequest(endpointId));

            return result.ToActionResult(this);
        }

        [AllowAnonymous]
        [HttpPost("{endpointId}")]
        public async Task<ActionResult<object>> PredictAsync([FromServices] ICustomAuthService customAuthService, [FromRoute] int endpointId)
        {
            var authenticationResult = await customAuthService.IsAuthenticatedAsync(this.Request);

            if (!authenticationResult.IsSuccess)
            {
                return Unauthorized("Requires authentication");
            }

            Result<object> result = await _mediator.Send(new GetPredictionRequest(endpointId, HttpContext.Request.Body));
            return result.ToActionResult(this);
        }

        [HttpPost("predictInputSample/{endpointId}")]
        public async Task<ActionResult<object>> PredictInputSample([FromRoute] int endpointId)
        {
            var result = await _mediator.Send(new GetPredictionRequest(endpointId, HttpContext.Request.Body, true));
            return result.ToActionResult(this);
        }
    }
}
