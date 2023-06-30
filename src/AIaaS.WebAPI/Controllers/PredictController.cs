using AIaaS.WebAPI.Interfaces;
using AIaaS.WebAPI.Models;
using AIaaS.WebAPI.Services;
using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIaaS.WebAPI.Controllers
{
    [Authorize(Policy = "Administrator")]
    [Route("api/[controller]")]
    [ApiController]
    public class PredictController : ControllerBase
    {
        private readonly IPredictionService _predictionService;

        public PredictController(IPredictionService predictionService)
        {
            _predictionService = predictionService;
        }

        [HttpGet("getPredictionInputSample/{endpointId}")]
        public async Task<IActionResult> GetPredictionInputSample([FromServices] IPredictionBuilderDirector builderDirector, [FromRoute] int endpointId)
        {
            var parameter = new PredictionParameter().SetEnpointId(endpointId);
            var result = await builderDirector.BuildInputSampleParameter(parameter);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors.FirstOrDefault());
            }

            var runtimeTypeInput = ClassFactory.CreateType(parameter.FeatureColumns);
            return Ok(Activator.CreateInstance(runtimeTypeInput));
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

            using var stream = new StreamReader(HttpContext.Request.Body);
            Result<object> serviceResult = await _predictionService.Predict(endpointId, stream);

            return serviceResult.ToActionResult(this);
            //switch (serviceResult.Status)
            //{
            //    case ResultStatus.Ok:
            //        return Ok(serviceResult.Value);
            //    case ResultStatus.Error:
            //        return BadRequest(serviceResult.Errors.FirstOrDefault());
            //    case ResultStatus.Forbidden:
            //        return Forbid();
            //    case ResultStatus.Unauthorized:
            //        return Unauthorized("Requires authentication");
            //    case ResultStatus.Invalid:
            //        return BadRequest(serviceResult.Errors.FirstOrDefault());
            //    case ResultStatus.NotFound:
            //        return NotFound();
            //    default: return BadRequest(serviceResult.Errors.FirstOrDefault());
            //}
        }

        [HttpPost("predictInputSample/{endpointId}")]
        public async Task<ActionResult<object>> PredictInputSample([FromRoute] int endpointId)
        {
            using var stream = new StreamReader(HttpContext.Request.Body);
            Result<object> serviceResult = await _predictionService.Predict(endpointId, stream, onlyPredictionProperties: true);

            return serviceResult.ToActionResult(this);
        }
    }

    public class PredictionInputDto
    {
        //public int EndpointId { get; set; }
        public string[]? Columns { get; set; }
        public object[]? Data { get; set; }
    }
}
