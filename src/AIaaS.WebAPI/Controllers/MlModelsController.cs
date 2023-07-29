using AIaaS.Application.Common.Models.Dtos;
using CleanArchitecture.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AIaaS.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MLModelsController : ControllerBase
    {
        private readonly IApplicationDbContext _dbContext;

        public MLModelsController(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var models = _dbContext.MLModels
                .Include(x => x.Workflow)
                .Include(x => x.Endpoint)
                .Select(x => new MlModelDto
                {
                    Name = x.Workflow.Name,
                    IsPublished = x.Endpoint != null,
                    Id = x.Id,
                    Size = x.Size,
                    CreatedBy = x.CreatedBy,
                    CreatedOn = x.CreatedOn,
                    ModifiedBy = x.ModifiedBy,
                    ModifiedOn = x.ModifiedOn
                })
                .ToList();

            return Ok(models);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var model = await _dbContext.MLModels
                .Include(x => x.Workflow)
                .Include(x => x.Endpoint)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (model is null)
            {
                return NotFound("Model not found");
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitizedFileName = string.Concat(model.Workflow.Name.Split(invalidChars));
            var normalizedFileName = sanitizedFileName.Replace(" ", "_") + ".zip";

            var dto = new MlModelDto
            {
                Name = model.Workflow.Name,
                FileName = normalizedFileName,
                IsPublished = model.Endpoint != null,
                Id = model.Id,
                Size = model.Size,
                WorkflowId = model.Workflow.Id,
                CreatedBy = model.CreatedBy,
                CreatedOn = model.CreatedOn,
                ModifiedBy = model.ModifiedBy,
                ModifiedOn = model.ModifiedOn
            };

            return Ok(dto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest("Id parameter should be greater than zero");

            var model = await _dbContext.MLModels.FindAsync(id);
            if (model is null)
                return NotFound();

            _dbContext.MLModels.Remove(model);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("getModelMetrics/{id}")]
        public async Task<IActionResult> GetModelMetrics(int id)
        {
            var metrics = await _dbContext.ModelMetrics.FindAsync(id);

            return metrics is null ? NotFound() : Ok(metrics);
        }

        [HttpGet("Download/{id:int}")]
        public async Task<IActionResult> Download(int id)
        {
            var model = await _dbContext.MLModels.FindAsync(id);
            if (model is null) return NotFound();

            return File(model.Data, "application/octet-stream");
        }
    }
}
