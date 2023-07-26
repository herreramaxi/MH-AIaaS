using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Application.Features.Datasets.Commands.CreateDataset;
using AIaaS.Application.Features.Datasets.Commands.RemoveDataset;
using AIaaS.Application.Features.Datasets.Commands.UploadDataset;
using AIaaS.Application.Features.Datasets.Queries;
using AIaaS.Application.Features.Datasets.Queries.GenerateFileAnalysis;
using AIaaS.Application.Features.Datasets.Queries.GenerateFilePreview;
using AIaaS.Application.Features.Datasets.Queries.GetDatasetFileStorage;
using AIaaS.Domain.Entities;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML.Data;
using System.Data;
using System.Net.Mime;

namespace AIaaS.WebAPI.Controllers
{
    [Authorize(Policy = "Administrator")]
    [Route("api/[controller]")]
    [ApiController]
    public class DatasetsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DatasetsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult> List()
        {
            var datasets = await _mediator.Send(new GetAllDatasetRequest());
            return Ok(datasets);
        }

        [HttpGet]
        [Route("{id:int:min(1)}")]
        public async Task<IActionResult> Get(int id)
        {
            var datasetDto = await _mediator.Send(new GetDatasetByIdRequest(id));
            if (datasetDto is null) return NotFound();

            return Ok(datasetDto);
        }

        [HttpGet("{datasetId:int}/columns")]
        public async Task<IActionResult> GetColumns([FromRoute] int datasetId)
        {
            var datasetDto = await _mediator.Send(new GetDatasetColumnsRequest(datasetId));
            if (datasetDto?.ColumnSettings is null) return NotFound();

            return Ok(datasetDto.ColumnSettings);
        }

        [HttpGet("GetAvailableDataTypes")]
        public ActionResult GetAvailableDataTypes()
        {
            var dataTypes = Enum.GetNames(typeof(DataKind));
            var dataTypesAsEnumeration = dataTypes.Select(x => new EnumerationDto() { Id = x, Name = x, Description = x });

            return Ok(dataTypesAsEnumeration);
        }

        [HttpGet("GetFilePreview/{datasetId:int}")]
        public async Task<ActionResult<DataViewFilePreviewDto>> GetFilePreview(int datasetId)
        {
            var result = await _mediator.Send(new GetDataViewFilePreviewRequest(datasetId));

            return result.ToActionResult(this);
        }

        [HttpGet("DownloadOriginalFile/{datasetId:int}")]
        public async Task<ActionResult<FileStorageDto>> DownloadOriginalFile(int datasetId)
        {
            var result = await _mediator.Send(new GetDatasetFileStorageRequest(datasetId));
            if (!result.IsSuccess)
            {
                return result.ToActionResult(this);
            }

            if (result.Value?.FileStream is null)
            {
                return BadRequest("Error when trying to download file: FileStream is null");
            }

            return new FileStreamResult(result.Value.FileStream, "application/octet-stream");
        }

        [HttpGet("DownloadBinaryIdvFile/{datasetId:int}")]
        public async Task<IActionResult> DownloadBinaryIdvFile(int datasetId)
        {
            var dataViewFileDto = await _mediator.Send(new GetDataViewFileRequest(datasetId));
            if (dataViewFileDto is null) return NotFound();

            return File(dataViewFileDto.Data, "application/octet-stream", dataViewFileDto.Name);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDatasetParameter parameter)
        {
            var dataset = await _mediator.Send(new CreateDatasetCommand(parameter));

            return Created("", dataset?.Id);
        }

        [HttpPost("Upload")]
        public async Task<ActionResult<Dataset>> Upload([FromForm] UploadFileStorageParameter parameter) //[FromForm] IFormFile file, int datasetId)
        {
            var dataset = await _mediator.Send(new UploadFileStorageCommand(parameter));

            return dataset.ToActionResult(this);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Remove([FromRoute] int id)
        {
            if (id <= 0)
                return BadRequest("Id parameter should be greater than zero");

            var result = await _mediator.Send(new RemoveDatasetCommand(id));

            return result.ToActionResult(this);
        }

        [HttpPost("Preview")]
        public async Task<ActionResult<FileAnalysisDto>> Preview([FromForm] GenerateFileAnalysisParameter parameter)
        {
            var result = await _mediator.Send(new GenerateFileAnalysisRequest(parameter));

            return result.ToActionResult(this);
        }
    }
}