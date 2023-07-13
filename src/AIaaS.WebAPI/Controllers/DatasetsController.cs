using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Domain.Entities;
using AIaaS.WebAPI.ExtensionMethods;
using CleanArchitecture.Application.Common.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using System.Data;
using System.Globalization;

namespace AIaaS.WebAPI.Controllers
{
    [Authorize(Policy = "Administrator")]
    [Route("api/[controller]")]
    [ApiController]
    public class DatasetsController : ControllerBase
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ILogger<DatasetsController> _logger;

        public DatasetsController(IApplicationDbContext dbContext, ILogger<DatasetsController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> List()
        {
            var datasets = await _dbContext.Datasets
                .OrderBy(x => x.Name)
                .ToListAsync();

            var dtos = datasets.Select(x => new DatasetDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                CreatedBy = x.CreatedBy,
                CreatedOn = x.CreatedOn,
                ModifiedBy = x.ModifiedBy,
                ModifiedOn = x.ModifiedOn
            });
            return Ok(dtos);
        }

        [HttpGet]
        [Route("{id:int:min(1)}")]
        public async Task<IActionResult> Get(int id)
        {
            var dataset = await _dbContext.Datasets
                .Include(x => x.FileStorage)
                .Include(x => x.DataViewFile)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dataset is null) return NotFound();

            var dto = new DatasetDto
            {
                Id = dataset.Id,
                Name = dataset.Name,
                Description = dataset.Description,
                Size = dataset.FileStorage?.Size,
                FileName = dataset.FileStorage?.FileName,
                DataViewFileName = dataset.DataViewFile?.Name,
                DataViewFileSize = dataset.DataViewFile?.Size,
                CreatedBy = dataset.CreatedBy,
                CreatedOn = dataset.CreatedOn,
                ModifiedBy = dataset.ModifiedBy,
                ModifiedOn = dataset.ModifiedOn
            };

            return Ok(dto);
        }

        [HttpGet("{datasetId:int}/columns")]
        public async Task<IActionResult> GetColumns([FromRoute] int datasetId)
        {
            var dataset = await _dbContext.Datasets
                .Include(x=> x.ColumnSettings)
                .FirstOrDefaultAsync(x=> x.Id == datasetId);

            if (dataset is null) { return NotFound(); }

            var columnSettings = dataset.ColumnSettings
                .Where(x => x.Include)
                .Select(x => new ColumnSettingDto
                {
                    Id = x.Id,
                    ColumnName = x.ColumnName,
                    Include = x.Include,
                    Type = x.Type
                });

            return Ok(columnSettings);
        }


        [HttpGet("GetAvailableDataTypes")]
        public ActionResult GetAvailableDataTypes()
        {
            var dataTypes = Enum.GetNames(typeof(DataKind));
            var dataTypesAsEnumeration = dataTypes.Select(x => new EnumerationDto() { Id = x, Name = x, Description = x });

            return Ok(dataTypesAsEnumeration);
        }

        [HttpGet("GetFilePreview/{datasetId:int}")]
        public async Task<ActionResult> GetFilePreview(int datasetId)
        {
            var dataset = await _dbContext.Datasets
                .Include(x=> x.DataViewFile)
                .FirstOrDefaultAsync(x=> x.Id == datasetId);

            if (dataset is null) return NotFound();
            if (dataset?.DataViewFile?.Data is null) return NotFound();

            using var memStream = new MemoryStream(dataset.DataViewFile.Data);
            var mss = new MultiStreamSourceFile(memStream);
            var mlContext = new MLContext();
            var dataview = mlContext.Data.LoadFromBinary(mss);
            var header = dataview.Schema.Select(x => x.Name);
            var MaxRows = 100;
            var preview = dataview.Preview(maxRows: MaxRows);
            var rowIndex = 0;
            var records = new List<string[]>();

            foreach (var row in preview.RowView)
            {
                var record = new string[row.Values.Length];
                var values = row.Values.Select(x => x.Value).ToArray();
                var ColumnCollection = row.Values;

                for (int i = 0; i < row.Values.Length; i++)
                {
                    record[i] = values[i]?.ToString() ?? "";
                }

                records.Add(record);

                rowIndex++;
            }

            return Ok(new
            {
                header = header,
                rows = records
            });

        }

        [HttpGet("DownloadOriginalFile/{datasetId:int}")]
        public async Task<IActionResult> DownloadOriginalFile(int datasetId)
        {
            var dataset = await _dbContext.Datasets
                .Include(x=> x.FileStorage)
                .FirstOrDefaultAsync(x=> x.Id == datasetId);

            if (dataset is null) return NotFound();
            if (dataset?.FileStorage?.Data is null) return NotFound();

            return File(dataset?.FileStorage?.Data, "application/octet-stream", dataset.FileStorage.FileName);
        }

        [HttpGet("DownloadBinaryIdvFile/{datasetId:int}")]
        public async Task<IActionResult> DownloadBinaryIdvFile(int datasetId)
        {
            var dataset = await _dbContext.Datasets
                .Include(x=> x.DataViewFile)
                .FirstOrDefaultAsync(x=> x.Id == datasetId);

            if (dataset is null) return NotFound();
            if (dataset?.DataViewFile?.Data is null) return NotFound();

            return File(dataset?.DataViewFile?.Data, "application/octet-stream", dataset.DataViewFile.Name);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DatasetDto datasetDto)
        {
            if (datasetDto is null)
                return BadRequest("Dataset is required");

            var dataset = new Dataset()
            {
                Name = datasetDto.Name,
                Description = datasetDto.Description,
                Delimiter = datasetDto.Delimiter,
                MissingRealsAsNaNs = datasetDto.MissingRealsAsNaNs
            };
            var columnSettings = datasetDto.ColumnSettings.Select(x => new ColumnSetting
            {
                ColumnName = x.ColumnName,
                Include = x.Include,
                Type = x.Type
            });

            dataset.ColumnSettings.AddRange(columnSettings);

            await _dbContext.Datasets.AddAsync(dataset);
            await _dbContext.SaveChangesAsync();

            return Created("", dataset.Id);
        }

        [HttpPost("Upload/{datasetId}")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, int datasetId)
        {
            var filePath = default(string);

            try
            {
                if (file is null)
                    return BadRequest("File is required");

                var dataset = await _dbContext.Datasets
                    .Include(x=> x.ColumnSettings)
                    .FirstOrDefaultAsync(x=> x.Id == datasetId);

                if (dataset is null)
                    return BadRequest("Dataset not found");

                filePath = await SaveTempFile(file);
                using var reader = file.OpenReadStream();
                using var memStream = new MemoryStream();
                await reader.CopyToAsync(memStream);

                var fileStorage = new FileStorage()
                {
                    FileName = file.FileName,
                    Size = file.Length,
                    Dataset = dataset,
                    Data = memStream.ToArray()
                };

                await _dbContext.FileStorages.AddAsync(fileStorage);
                await _dbContext.SaveChangesAsync();

                reader.Seek(0, SeekOrigin.Begin);

                var mlContext = new MLContext();
                var columns = dataset.ColumnSettings.Select((x, index) => new TextLoader.Column(x.ColumnName, x.Type.ToDataKind(), index)).ToArray();
                var separator = dataset.Delimiter.ToCharDelimiter();
                var options = new TextLoader.Options
                {
                    AllowQuoting = true,
                    HasHeader = true,
                    MissingRealsAsNaNs = dataset.MissingRealsAsNaNs ?? false,
                    Separators = new[] { separator },
                    Columns = columns
                };

                var columnNames = dataset.ColumnSettings.Select(x => x.ColumnName);
                var labelColumn = columnNames
                    .Where(x => x.Equals("Label", StringComparison.InvariantCultureIgnoreCase))
                  .FirstOrDefault() ?? columnNames.Last();

                var dataView = mlContext.Data.LoadFromTextFile(filePath, options: options);
                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                var dataViewFileName = $"{fileName}.idv";
                using var stream = new MemoryStream();
                mlContext.Data.SaveAsBinary(dataView, stream);

                var dataview = new DataViewFile
                {
                    Name = dataViewFileName,
                    Size = stream.Length,
                    Dataset = dataset,
                    Data = stream.ToArray()
                };

                await _dbContext.DataViewFiles.AddAsync(dataview);
                await _dbContext.SaveChangesAsync();

                return Created("", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when uploading file");
                return StatusCode(500, $"Error when uploading file: {ex.Message}");
            }
            finally
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Remove([FromRoute] int id)
        {
            if (id <= 0)
                return BadRequest("Id parameter should be greater than zero");

            var dataset = await _dbContext.Datasets.FirstOrDefaultAsync(x => x.Id == id);
            if (dataset is null)
                return NotFound();

            _dbContext.Datasets.Remove(dataset);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("Preview")]
        public async Task<IActionResult> Preview([FromForm] IFormFile file, [FromForm] string? delimiter = null, [FromForm] bool missingRealsAsNaNs = false)
        {
            var filePath = default(string);
            try
            {
                if (file is null)
                    return BadRequest("File is required");

                var fileAnalysis = new FileAnalysisDto();
                fileAnalysis.Delimiter = !string.IsNullOrEmpty(delimiter) ?
                    delimiter.ToStringDelimiter() :
                    file.FileName.Contains(".tsv", StringComparison.InvariantCultureIgnoreCase) ? "\t" : ",";

                filePath = await SaveTempFile(file);

                fileAnalysis.Header = GetHeader(file, fileAnalysis);

                if (!fileAnalysis.Header.Any())
                {
                    return BadRequest("Header is empty");
                }

                var labelColumn = fileAnalysis.Header
                    .Where(x => x.Equals("Label", StringComparison.InvariantCultureIgnoreCase))
                    .FirstOrDefault() ?? fileAnalysis.Header.Last();

                var mlContext = new MLContext();
                var separatorChar = fileAnalysis.Delimiter.ToCharDelimiter();
                var columnInference = mlContext.Auto().InferColumns(filePath, labelColumnName: labelColumn, groupColumns: false, separatorChar: separatorChar);
                columnInference.TextLoaderOptions.MissingRealsAsNaNs = missingRealsAsNaNs;

                fileAnalysis.ColumnsSettings = columnInference.TextLoaderOptions.Columns.Select(x =>
                      new ColumnSettingDto
                      {
                          ColumnName = x.Name,
                          Include = true,
                          Type = x.DataKind.ToString(),
                      }
                     ).ToList();

                TextLoader loader = mlContext.Data.CreateTextLoader(columnInference.TextLoaderOptions);
                IDataView data = loader.Load(filePath);

                var MaxRows = 100;
                var preview = data.Preview(maxRows: MaxRows);

                var records = new List<string[]>();

                foreach (var row in preview.RowView)
                {
                    var record = new string[row.Values.Length];
                    var values = row.Values.Select(x => x.Value).ToArray();
                    var ColumnCollection = row.Values;

                    for (int i = 0; i < row.Values.Length; i++)
                    {
                        record[i] = values[i]?.ToString() ?? "";
                    }

                    records.Add(record);
                }

                fileAnalysis.Data = records.ToArray();

                fileAnalysis.Delimiter = fileAnalysis.Delimiter.Replace("\t", "\\t");
                return Ok(fileAnalysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when generating file analysis");
                return StatusCode(500, $"Error when generating file preview: {ex.Message}");
            }
            finally
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }

        private string[] GetHeader(IFormFile file, FileAnalysisDto fileAnalysis)
        {
            using var reader = file.OpenReadStream();
            using var tr = new StreamReader(reader);
            var csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture);
            csvConfiguration.Delimiter = fileAnalysis.Delimiter;
            csvConfiguration.HasHeaderRecord = true;
            using var csv = new CsvReader(tr, csvConfiguration);
            csv.Read();
            csv.ReadHeader();
            return csv.Context.Reader.HeaderRecord ?? Array.Empty<string>();
        }

        private async Task<string> SaveTempFile(IFormFile file)
        {
            var filePath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv";
            using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);
            await fileStream.FlushAsync();

            return filePath;
        }
    }
}