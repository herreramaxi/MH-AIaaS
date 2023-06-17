using AIaaS.WebAPI.Models.Dtos;
using AIaaS.WebAPI.Models.enums;
using AIaaS.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML.Data;

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
            var enums = new List<EnumerationDto>()
            {
                new EnumerationDto{Id = Microsoft.ML.Transforms.MissingValueReplacingEstimator.ReplacementMode.DefaultValue.ToString(), Name = "Default", Description="Replace with the default value of the column based on its type" },
                new EnumerationDto{Id = Microsoft.ML.Transforms.MissingValueReplacingEstimator.ReplacementMode.Maximum.ToString(), Name = "Maximum", Description="Replace with the maximum value of the column" },
                new EnumerationDto{Id = Microsoft.ML.Transforms.MissingValueReplacingEstimator.ReplacementMode.Mean.ToString(), Name = "Mean", Description="Replace with the mean value of the column" },
                new EnumerationDto{Id = Microsoft.ML.Transforms.MissingValueReplacingEstimator.ReplacementMode.Minimum.ToString(), Name = "Minimum", Description="Replace with the minimum value of the column" },
                new EnumerationDto{Id = Microsoft.ML.Transforms.MissingValueReplacingEstimator.ReplacementMode.Mode.ToString(), Name = "Most frequent value", Description=" Replace with the most frequent value of the column" },
                //new EnumerationDto{Id = "RemoveRow", Name = "Remove row", Description="Remove row if any cell contains a missing value" },
            };
            return Ok(enums);
        }

        [HttpGet("GetNormalizerModes")]
        public ActionResult GetNormalizerModes()
        {
            var enums = new List<EnumerationDto>()
            {
                new EnumerationDto{Id = NormalizationTypeEnum.Binning.ToString(), Name = "Binning", Description="Binning" },
                new EnumerationDto{Id = NormalizationTypeEnum.LogMeanVariance.ToString(), Name = "Log Mean Variance", Description="Log Mean Variance" },
                new EnumerationDto{Id = NormalizationTypeEnum.MinMax.ToString(), Name = "Min-Max", Description="Min-Max" },
                 new EnumerationDto{Id = NormalizationTypeEnum.RobustScaling.ToString(), Name = "Robust Scaling", Description="Robust Scaling" },
                 new EnumerationDto{Id = NormalizationTypeEnum.ZScore.ToString(), Name = "Z-Score", Description="Z-Score" }
            };

            return Ok(enums);
        }

        [HttpGet("GetMLTasks")]
        public ActionResult GetMLTasks()
        {
            var enums = new List<EnumerationDto>()
            {
                new EnumerationDto{Id =  "Regression", Name = "Regression", Description="Regression" },
                new EnumerationDto{Id =  "BinaryClassification", Name =  "Binary Classification", Description="Binary Classification" }
            };

            return Ok(enums);
        }

        [HttpGet("GetTrainers")]
        public ActionResult GetTrainers(string task)
        {
            var enums = new List<EnumerationDto>();
            if (task.Equals("Regression", StringComparison.InvariantCultureIgnoreCase))
            {
                enums.Add(new EnumerationDto { Id = "SdcaRegression", Name = "SDCA Regression", Description = "Stochastic dual coordinate ascent regression" });
                enums.Add(new EnumerationDto { Id = "Ols", Name = "Ordinary Least Squares (OLS)", Description = "Ordinary Least Squares (OLS)" });
                enums.Add(new EnumerationDto { Id = "OnlineGradientDescent", Name = "Online Gradient Descent", Description = "Online Gradient Descent" });

            }
            else if (task.Equals("BinaryClassification", StringComparison.InvariantCultureIgnoreCase))
            {
                enums.Add(new EnumerationDto { Id = "SdcaLogisticRegression", Name = "Sdca Logistic Regression" });
                enums.Add(new EnumerationDto { Id = "LinearSvm", Name = "Linear Svm" });
                enums.Add(new EnumerationDto { Id = "AveragedPerceptron", Name = "Averaged Perceptron" });
                enums.Add(new EnumerationDto { Id = "FastTree", Name = "FastTree" });                
            }
            else
            {
                return NotFound("No trainers found for task");
            }

            return Ok(enums);
        }

        [HttpGet("GetAvailableDataTypes")]
        public ActionResult GetAvailableDataTypes()
        {
            var dataTypes = new List<EnumerationDto>
            {
            new EnumerationDto{Id = DataKind.String.ToString(), Name ="String" } ,
            new EnumerationDto{Id = DataKind.Int32.ToString(), Name ="Integer" } ,
            new EnumerationDto{Id = DataKind.Single.ToString(), Name ="Float" } ,
            new EnumerationDto{Id = DataKind.Double.ToString(), Name ="Double" } ,
            new EnumerationDto{Id = DataKind.Boolean.ToString(), Name ="Boolean" } ,
            new EnumerationDto{Id = DataKind.DateTime.ToString(), Name ="DateTime" }
            };

            return Ok(dataTypes);
        }


        [HttpGet("GetCategoricalTypes")]
        public ActionResult GetCategoricalTypes()
        {
            var dataTypes = new List<EnumerationDto>
            {
            new EnumerationDto{Id = "OneHotEncoding", Name = "OneHotEncoding" } ,
            new EnumerationDto{Id ="OneHotHashEncoding", Name ="OneHotHashEncoding" }
            };

            return Ok(dataTypes);
        }
    }
}
