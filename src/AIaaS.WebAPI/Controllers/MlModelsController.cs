using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AIaaS.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MLModelsController : ControllerBase
    {
        public static List<MlModelDto> _models = new List<MlModelDto>()
            {
                new MlModelDto(){ Id = 1, Name= "Housing - Price Prediction" , Status = "Published", CreatedBy = "Maxi Herrera", CreatedOn = DateTime.Now, ModifiedBy = "Herrera Maxi" , ModifiedOn = DateTime.Now},
                new MlModelDto(){ Id = 2, Name= "Flower Types - Classification" , Status = "Running", CreatedBy = "Maxi Herrera", CreatedOn = DateTime.Now, ModifiedBy = "Herrera Maxi" , ModifiedOn = DateTime.Now},
                new MlModelDto(){ Id = 3, Name= "Advertising - Sales Prediction" , Status = "Submitted", CreatedBy = "Maxi Herrera", CreatedOn = DateTime.Now, ModifiedBy = "Herrera Maxi" , ModifiedOn = DateTime.Now}
            };
        private readonly EfContext _dbContext;

        public MLModelsController(EfContext dbContext)
        {
            _dbContext = dbContext;

            var workflow = _dbContext.Workflows.FirstOrDefault(x => x.Name == "Housing - Price Prediction");

            _models.ForEach(x => x.Workflow = workflow);
        }

        // GET: api/<ModelsController>
        [HttpGet]
        public IEnumerable<MlModelDto> Get()
        {
            return _models;
        }

        // GET api/<ModelsController>/5
        [HttpGet("{id}")]
        public MlModelDto? Get(int id)
        {
            return _models.FirstOrDefault(x => x.Id == id);
        }

        // POST api/<ModelsController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ModelsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ModelsController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var model = _models.FirstOrDefault(x => x.Id == id);
            if (model == null)
            {
                return NotFound();
            }

            _models.Remove(model);

            return Ok();
        }

        [HttpGet("getModelMetrics/{id}")]
        public async Task<IActionResult> GetModelMetrics(int id)
        {
            var metrics = await _dbContext.ModelMetrics.FindAsync(id);

            return metrics is null ? NotFound() : Ok(metrics);
        }
    }
}
