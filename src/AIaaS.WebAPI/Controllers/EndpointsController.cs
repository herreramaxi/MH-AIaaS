using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AIaaS.WebAPI.Controllers
{
    [Authorize(Policy = "Administrator")]
    [Route("api/[controller]")]
    [ApiController]
    public class EndpointsController : ControllerBase
    {
        private static List<EndpointDto> _endpoints;
        private readonly EfContext _dbContext;

        public EndpointsController(EfContext dbContext)
        {
            _dbContext = dbContext;

            var workflow = _dbContext.Workflows.SingleOrDefault(x => x.Name == "Housing - Price Prediction");            
            var model = MlModelsController._models.FirstOrDefault(x => x.Id == 1);

            _endpoints = new List<EndpointDto>()
            {
                new EndpointDto { Id = 1, Name = "Endpoint HPP",  ModelId = model.Id,ModelName = model.Name , WorkflowId = workflow.Id , WorkflowName = workflow.Name , CreatedBy = "Herrera Maxi", CreatedOn= DateTime.Now, IsEnabled = true, ApiKey = "ZDk0YjUyZDYtZWQxNi00NWQwLTg2ZGUtOGM3NjBhNWM2Njcx" },
                new EndpointDto { Id = 2, Name = "Endpoint Flower Types",  ModelId = 2,ModelName = "Flower Types - Classification", WorkflowId =3 , WorkflowName = "Flower Types - Classification", CreatedBy = "Herrera Maxi", CreatedOn= DateTime.Now,IsEnabled = true, ApiKey = "NWRmMjc5ZGEtMzUyMi00MGQ2LTg3NDYtNmUxNGUzZjMxZjNl" },
            };
           
        }
        // GET: api/<EndpointsController>
        [HttpGet]
        public IEnumerable<EndpointDto> Get()
        {
            return _endpoints;
        }

        // GET api/<EndpointsController>/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var endpoint = _endpoints.FirstOrDefault(x => x.Id == id);

            if (endpoint is null)
                return NotFound();

            return Ok(endpoint);
        }

        // POST api/<EndpointsController>
        [HttpPost]
        public void Post([FromBody] EndpointDto endpoint)
        {
            _endpoints.Add(endpoint);
        }

        // PUT api/<EndpointsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<EndpointsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {            
        }
    }
}
