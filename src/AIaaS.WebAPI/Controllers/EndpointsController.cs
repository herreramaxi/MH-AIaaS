using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Interfaces;
using AIaaS.WebAPI.Models;
using AIaaS.WebAPI.Models.Dtos;
using AIaaS.WebAPI.Models.enums;
using Ardalis.Result.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AIaaS.WebAPI.Controllers
{
    [Authorize(Policy = "Administrator")]
    [Route("api/[controller]")]
    [ApiController]
    public class EndpointsController : ControllerBase
    {
        private readonly EfContext _dbContext;

        public EndpointsController(EfContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var endpointsDto = (await _dbContext.Endpoints.ToListAsync())
                .Select((x) =>
                {
                    _dbContext.Entry(x).Reference(y => y.MLModel).Load();

                    if (x.MLModel is not null)
                        _dbContext.Entry(x.MLModel).Reference(y => y.Workflow).Load();

                    var dto = new EndpointDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        IsEnabled = x.IsEnabled,
                        AuthenticationType = x.AuthenticationType,
                        CreatedBy = x.CreatedBy,
                        CreatedOn = x.CreatedOn,
                        ModifiedBy = x.ModifiedBy,
                        ModifiedOn = x.ModifiedOn,
                        ModelId = x.MLModel?.Id,
                        WorkflowId = x.MLModel?.Workflow?.Id ?? 0,
                        WorkflowName = x.MLModel?.Workflow?.Name
                    };

                    return dto;
                });

            return Ok(endpointsDto);
        }

        // GET api/<EndpointsController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var endpoint = await _dbContext.Endpoints.FindAsync(id);

            if (endpoint is null)
                return NotFound();

            await _dbContext.Entry(endpoint).Reference(y => y.MLModel).LoadAsync();

            if (endpoint.MLModel is not null)
                await _dbContext.Entry(endpoint.MLModel).Reference(y => y.Workflow).LoadAsync();

            var dto = new
            {
                endpoint.Id,
                endpoint.Name,
                endpoint.Description,
                endpoint.CreatedBy,
                endpoint.CreatedOn,
                endpoint.ModifiedBy,
                endpoint.ModifiedOn,
                endpoint.IsEnabled,
                ModelId = endpoint.MLModel?.Id,
                WorkflowId = endpoint.MLModel?.Workflow.Id,
                WorkflowName = endpoint.MLModel?.Workflow.Name
            };

            return Ok(dto);
        }


        [HttpGet("getAuthenticationInfo/{id}")]
        public async Task<IActionResult> GetAuthenticationInfo(int id)
        {
            var endpoint = await _dbContext.Endpoints.FindAsync(id);

            if (endpoint is null)
                return NotFound();

            var dto = new
            {
                endpoint.Id,
                endpoint.AuthenticationType,
                endpoint.ApiKey
            };

            return Ok(dto);
        }

        [HttpGet("tokenBasedGetToken")]
        public IActionResult TokenBasedGetToken()
        {
            var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
            return Ok(new { token = token });
        }

        [HttpGet("GetAuthenticationTypes")]
        public ActionResult GetAuthenticationTypes()
        {
            var enums = new List<EnumerationDto>()
            {
                //new EnumerationDto{Id =  ((int)AuthenticationType.Basic).ToString(), Name = "Basic"},
                new EnumerationDto{Id =  ((int)AuthenticationType.TokenBased).ToString(), Name =  "Token-Based" },
                new EnumerationDto{Id =  ((int)AuthenticationType.JWT).ToString(), Name =  "JWT" }
            };

            return Ok(enums);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] EndpointDto endpointDto)
        {
            var workflow = await _dbContext.Workflows.FindAsync(endpointDto.WorkflowId);

            if (workflow is null)
            {
                return BadRequest("Workflow not found");
            }

            await _dbContext.Entry(workflow).Reference(x => x.MLModel).LoadAsync();

            if (workflow.MLModel is null)
            {
                return BadRequest("Model not found, please generate the model before creating the endpoint");
            }

            var endpoint = new MLEndpoint
            {
                Name = endpointDto.Name,
                Description = endpointDto.Description,
                IsEnabled = true,
                MLModel = workflow.MLModel,
                AuthenticationType = endpointDto.AuthenticationType,
                ApiKey = endpointDto.AuthenticationType == AuthenticationType.TokenBased ? Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())) : null
            };

            await _dbContext.Endpoints.AddAsync(endpoint);
            await _dbContext.SaveChangesAsync();

            endpointDto.Id = endpoint.Id;
            endpointDto.IsEnabled = endpoint.IsEnabled;
            endpointDto.CreatedBy = endpoint.CreatedBy;
            endpointDto.CreatedOn = endpoint.CreatedOn;
            endpointDto.ApiKey = endpoint.ApiKey;

            return Ok(endpointDto);
        }

        // PUT api/<EndpointsController>/5
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] EndpointDto endpointDto)
        {
            if (endpointDto.Id is null)
            {
                return BadRequest("Endpoint.Id is required");
            }

            var endpoint = await _dbContext.Endpoints.FindAsync(endpointDto.Id);

            if (endpoint is null)
                return NotFound();

            endpoint.Name = endpointDto.Name;
            endpoint.Description = endpointDto.Description;
            endpoint.IsEnabled = endpointDto.IsEnabled;
            endpoint.AuthenticationType = endpointDto.AuthenticationType;
            endpoint.ApiKey = endpointDto.ApiKey;

            await _dbContext.SaveChangesAsync();

            endpointDto.ModifiedBy = endpoint.ModifiedBy;
            endpointDto.ModifiedOn = endpoint.ModifiedOn;

            return Ok(endpointDto);
        }

        [HttpPut("updateAuthentication")]
        public async Task<IActionResult> UpdateAuthentication([FromBody] EndpointAuthenticationDto endpointAuthenticationDto)
        {
            if (endpointAuthenticationDto.Id <= 0)
            {
                return BadRequest("Endpoint.Id should be greater than zero");
            }

            var endpoint = await _dbContext.Endpoints.FindAsync(endpointAuthenticationDto.Id);

            if (endpoint is null)
                return NotFound();

            endpoint.AuthenticationType = endpointAuthenticationDto.AuthenticationType;
            endpoint.ApiKey = endpointAuthenticationDto.ApiKey;

            await _dbContext.SaveChangesAsync();

            return Ok(endpointAuthenticationDto);
        }

        // DELETE api/<EndpointsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
