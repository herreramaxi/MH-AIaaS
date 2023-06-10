using AIaaS.WebAPI.Models.Dtos;
using AIaaS.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIaaS.WebAPI.Controllers
{
    [Authorize(Policy = "Administrator")]
    [Route("api/[controller]")]
    [ApiController]
    public class OperatorsController : ControllerBase
    {
        private readonly IOperatorService _operatorService;

        public OperatorsController(IOperatorService operatorService)
        {
            _operatorService = operatorService;
        }

        [HttpGet]
        public ActionResult Get()
        {
            var operators = _operatorService.GetOperators();
            return Ok(operators);
        }

        [HttpGet("GetCleaningModes")]
        public ActionResult GetCleaningModes()
        {
            var replacementModes = new List<CleaningModeDto>()
            {
                new CleaningModeDto{Id = (int)Microsoft.ML.Transforms.MissingValueReplacingEstimator.ReplacementMode.DefaultValue, Name = "Default", Description="Replace with the default value of the column based on its type" },
                new CleaningModeDto{Id = (int)Microsoft.ML.Transforms.MissingValueReplacingEstimator.ReplacementMode.Mean, Name = "Mean", Description="Replace with the mean value of the column" },
                new CleaningModeDto{Id = (int)Microsoft.ML.Transforms.MissingValueReplacingEstimator.ReplacementMode.Minimum, Name = "Minimum", Description="Replace with the minimum value of the column" },
                new CleaningModeDto{Id = (int)Microsoft.ML.Transforms.MissingValueReplacingEstimator.ReplacementMode.Maximum, Name = "Maximum", Description="Replace with the maximum value of the column" },
                new CleaningModeDto{Id = (int)Microsoft.ML.Transforms.MissingValueReplacingEstimator.ReplacementMode.Mode, Name = "Most frequent value", Description=" Replace with the most frequent value of the column" },
                new CleaningModeDto{Id = 6, Name = "Remove row", Description="Remove row if any cell contains a missing value" },
            };
            return Ok(replacementModes);
        }
    }
}
