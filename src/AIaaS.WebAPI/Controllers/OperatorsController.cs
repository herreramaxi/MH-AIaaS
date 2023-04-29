using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIaaS.WebAPI.Controllers
{
    [Authorize(Policy = "Administrator")]
    [Route("api/[controller]")]
    [ApiController]
    public class OperatorsController : ControllerBase
    {
        private readonly EfContext _dbContext;

        public OperatorsController(EfContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var dtos = new List<OperatorDto>() {
            new OperatorDto { Name = "Dataset",
                Type = "dataset",
                Data = new OperatorDataDto{
                    Name = "Dataset",
                    Config = new List<OperatorConfigurationDto>(){
                    new OperatorConfigurationDto{
                        Name = "Variable Name",
                        Description = "The name of the new or existing variable",
                        Type = "text"
                    },
                          new OperatorConfigurationDto{
                        Name = "Value",
                        Description = "Javascript expression for the value",
                        Type = "text"
                    },
                    }

            } },
               new OperatorDto { Name = "Clean Data",
                Type = "clean",
                Data = new OperatorDataDto{
                    Name = "Clean Data",
                    Config = new List<OperatorConfigurationDto>(){
                    new OperatorConfigurationDto{
                        Name = "Variable Name",
                        Description = "The name of the new or existing variable",
                        Type = "text"
                    },
                          new OperatorConfigurationDto{
                        Name = "Value",
                        Description = "Javascript expression for the value",
                        Type = "text"
                    },
                    }

            } },
               new OperatorDto { Name = "Split Data",
                Type = "split",
                Data = new OperatorDataDto{
                    Name = "Split Data",
                    Config = new List<OperatorConfigurationDto>(){
                    new OperatorConfigurationDto{
                        Name = "Variable Name",
                        Description = "The name of the new or existing variable",
                        Type = "text"
                    },
                          new OperatorConfigurationDto{
                        Name = "Value",
                        Description = "Javascript expression for the value",
                        Type = "text"
                    },
                    }

            } },
               new OperatorDto { Name = "Train Model",
                Type = "train",
                Data = new OperatorDataDto{
                    Name = "Train Mode",
                    Config = new List<OperatorConfigurationDto>(){
                    new OperatorConfigurationDto{
                        Name = "Variable Name",
                        Description = "The name of the new or existing variable",
                        Type = "text"
                    },
                          new OperatorConfigurationDto{
                        Name = "Value",
                        Description = "Javascript expression for the value",
                        Type = "text"
                    },
                    }

            } },
               new OperatorDto { Name = "Evaluate",
                Type = "evaluate",
                Data = new OperatorDataDto{
                    Name = "Evaluate",
                    Config = new List<OperatorConfigurationDto>(){
                    new OperatorConfigurationDto{
                        Name = "Variable Name",
                        Description = "The name of the new or existing variable",
                        Type = "text"
                    },
                          new OperatorConfigurationDto{
                        Name = "Value",
                        Description = "Javascript expression for the value",
                        Type = "text"
                    },
                    }

            } }
            };

            return Ok(dtos);
        }
    }
}
