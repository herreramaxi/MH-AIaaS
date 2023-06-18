using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.ExtensionMethods;
using AIaaS.WebAPI.Models;
using AIaaS.WebAPI.Models.Dtos;
using AIaaS.WebAPI.Models.enums;
using AIaaS.WebAPI.Models.Operators;
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
using System.Security.Claims;

namespace AIaaS.WebAPI.Controllers
{
    [Authorize(Policy = "Administrator")]
    [Route("api/[controller]")]
    [ApiController]
    public class DatasetsController : ControllerBase
    {
        private readonly EfContext _dbContext;
        private readonly ILogger<DatasetsController> _logger;

        public DatasetsController(EfContext dbContext, ILogger<DatasetsController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
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
                ModifiedOn = x.ModifiedOn,
                ColumnSettings = x.ColumnSettings.OrderBy(c => c.ColumnName).Select(x => new ColumnSettingDto
                {
                    ColumnName = x.ColumnName,
                    Id = x.Id,
                    Include = x.Include,
                    Type = x.Type
                }).ToList()
            });
            return Ok(dtos);
        }

        [HttpGet("{datasetId:int}/columns")]
        public async Task<IActionResult> GetColumns([FromRoute] int datasetId)
        {
            var dataset = await _dbContext.Datasets.FindAsync(datasetId);
            if (dataset is null) { return NotFound(); }

            _dbContext.Entry(dataset).Collection(p => p.ColumnSettings).Load();

            var columnSettings = dataset.ColumnSettings
                .Where(x => x.Include)
                .Select(x => new ColumnSettingDto
                {
                    Id = x.Id,
                    ColumnName = x.ColumnName,
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

        [HttpPost]
        public async Task<IActionResult> Create(DatasetDto datasetDto)
        {
            if (datasetDto is null)
                return BadRequest("Dataset is required");
            var userEmail = this.User.FindFirst(ClaimTypes.Email)?.Value;

            if (userEmail == null)
                return BadRequest("User email not found on request");

            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email.ToLower().Equals(userEmail.ToLower()));

            if (user == null)
                return BadRequest("User not found");

            var dataset = new Dataset()
            {
                Name = datasetDto.Name,
                Description = datasetDto.Description,
                Delimiter = datasetDto.Delimiter,
                MissingRealsAsNaNs = datasetDto.MissingRealsAsNaNs,
                User = user
            };

            var columnSettings = datasetDto.ColumnSettings.Select(x => new ColumnSetting
            {
                ColumnName = x.ColumnName,
                Include = x.Include,
                Type = x.Type
            });

            dataset.ColumnSettings.AddRange(columnSettings);

            await _dbContext.AddAsync(dataset);
            await _dbContext.SaveChangesAsync();

            return Created("", dataset.Id);
        }

        public void NewMethod<T>(MLContext mlContext, StreamReader tr) where T : class
        {
            var csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture);
            var csv = new CsvReader(tr, csvConfiguration);

            var records = csv.GetRecords<T>();

            var data = mlContext.Data.LoadFromEnumerable(records);
            //var preview = data.Preview();
            using (FileStream stream = new FileStream("data.idv", FileMode.Create))
                mlContext.Data.SaveAsBinary(data, stream);
        }


        public IDataView NewMethod2<T>(MLContext mlContext, StreamReader tr) where T : class
        {
            var csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture);
            var csv = new CsvReader(tr, csvConfiguration);
            //TODO: I think I cannot use csv reader
            var records = csv.GetRecords<T>().ToList();
            var data = mlContext.Data.LoadFromEnumerable(records);

            return data;
        }

        [HttpPost("UploadTest/{datasetId}")]
        public async Task<IActionResult> UploadTest([FromForm] IFormFile file, int datasetId)
        {
            if (file is null)
                return BadRequest("File is required");

            var dataset = await _dbContext.Datasets.FindAsync(datasetId);

            if (dataset is null)
                return BadRequest("Dataset not found");

            await _dbContext.Entry(dataset).Collection(x => x.ColumnSettings).LoadAsync();
            var propertyTypes = dataset.ColumnSettings.Select(x => (x.ColumnName, x.Type.ToDataType()));

            var mlContext = new MLContext(seed: 0);


            //mlContext.Auto().InferColumns()
            //mlContext.Data.SaveAsBinary(data, stream);
            //mlContext.Data.LoadFromEnumerable

            using var reader = file.OpenReadStream();
            using var tr = new StreamReader(reader);

            var isGeneric = true;

            if (isGeneric)
            {
                var rowType = ClassFactory.CreateType(propertyTypes);
                var methodInfo = this.GetType().GetMethods().FirstOrDefault(x => x.Name == "NewMethod");
                var processMethod = methodInfo.MakeGenericMethod(rowType);// predictionObject.GetType());
                processMethod.Invoke(this, new object[] { mlContext, tr });
            }
            else
            {
                var csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture);
                var csv = new CsvReader(tr, csvConfiguration);

                var records = csv.GetRecords<SalesRow>();

                var data = mlContext.Data.LoadFromEnumerable(records);
                //var preview = data.Preview();
                using (FileStream stream = new FileStream("data2.idv", FileMode.Create))
                {
                    mlContext.Data.SaveAsBinary(data, stream);
                    stream.Flush();
                }


            }
            //using var memStream = new MemoryStream();
            //await reader.CopyToAsync(memStream);

            //var fileStorage = new FileStorage()
            //{
            //    FileName = file.FileName,
            //    Size = file.Length,
            //    Dataset = dataset
            //};          

            //fileStorage.Data = memStream.ToArray();
            //await _dbContext.FileStorages.AddAsync(fileStorage);
            //await _dbContext.SaveChangesAsync();

            return Created("", null);
        }

        [HttpPost("UploadOld/{datasetId}")]
        public async Task<IActionResult> UploadOld([FromForm] IFormFile file, int datasetId)
        {
            if (file is null)
                return BadRequest("File is required");

            var dataset = await _dbContext.Datasets.FindAsync(datasetId);

            if (dataset is null)
                return BadRequest("Dataset not found");

            await _dbContext.Entry(dataset).Collection(x => x.ColumnSettings).LoadAsync();
            var propertyTypes = dataset.ColumnSettings.Select(x => (x.ColumnName, x.Type.ToDataType()));

            using var reader = file.OpenReadStream();
            using var tr = new StreamReader(reader);
            using var memStream = new MemoryStream();
            await reader.CopyToAsync(memStream);

            var fileStorage = new FileStorage()
            {
                FileName = file.FileName,
                Size = file.Length,
                Dataset = dataset
            };

            fileStorage.Data = memStream.ToArray();
            await _dbContext.FileStorages.AddAsync(fileStorage);
            await _dbContext.SaveChangesAsync();

            reader.Seek(0, SeekOrigin.Begin);

            var mlContext = new MLContext();

            //var propertyTypes = columnSettings.Select(x => (x.ColumnName, x.Type.ToDataType()));
            var rowType = ClassFactory.CreateType(propertyTypes);


            var methodInfo = this.GetType().GetMethods().FirstOrDefault(x => x.Name == "NewMethod2");
            var processMethod = methodInfo.MakeGenericMethod(rowType);
            var baseTrainingDataView = processMethod.Invoke(this, new object[] { mlContext, tr }) as IDataView;

            using var stream = new MemoryStream();
            mlContext.Data.SaveAsBinary(baseTrainingDataView, stream);

            var dataview = new DataViewFile()
            {
                Size = stream.Length,
                Dataset = dataset
            };

            dataview.Data = stream.ToArray();
            await _dbContext.DataViewFiles.AddAsync(dataview);
            await _dbContext.SaveChangesAsync();

            return Created("", null);
        }


        [HttpPost("Upload/{datasetId}")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, int datasetId)
        {
            var filePath = default(string);

            try
            {
                if (file is null)
                    return BadRequest("File is required");

                var dataset = await _dbContext.Datasets.FindAsync(datasetId);

                if (dataset is null)
                    return BadRequest("Dataset not found");

                await _dbContext.Entry(dataset).Collection(x => x.ColumnSettings).LoadAsync();

                filePath = await SaveTempFile(file);

                using var reader = file.OpenReadStream();
                using var tr = new StreamReader(reader);
                using var memStream = new MemoryStream();
                await reader.CopyToAsync(memStream);

                var fileStorage = new FileStorage()
                {
                    FileName = file.FileName,
                    Size = file.Length,
                    Dataset = dataset
                };

                fileStorage.Data = memStream.ToArray();
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

                //var columnInference = mlContext.Auto().InferColumns(filePath, labelColumnName: labelColumn, groupColumns: false, separatorChar: separator);
                //columnInference.TextLoaderOptions.MissingRealsAsNaNs = dataset.MissingRealsAsNaNs??false;
                //TextLoader loader = mlContext.Data.CreateTextLoader(columnInference.TextLoaderOptions);
                //IDataView dataView = loader.Load(filePath);
                //fileAnalysis.Header = fileAnalysis.ColumnsSettings.Select(x => x.ColumnName).ToArray();


                //TextLoader loader = mlContext.Data.CreateTextLoader(options);
                //var dataView = loader.Load(filePath);

                var dataView = mlContext.Data.LoadFromTextFile(filePath, options: options);

                using var stream = new MemoryStream();
                mlContext.Data.SaveAsBinary(dataView, stream);

                var dataview = new DataViewFile
                {
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

        [HttpPost("PreviewOld")]
        public IActionResult PreviewOld([FromForm] IFormFile file)
        {
            //var file = Request.Form.Files[0];
            if (file is null)
                return BadRequest("FIle is required");

            using var reader = file.OpenReadStream();
            using var tr = new StreamReader(reader);
            var csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture);
            //csvConfiguration.Delimiter = ",";
            csvConfiguration.HasHeaderRecord = true;
            csvConfiguration.DetectDelimiter = true;

            using var csv = new CsvReader(tr, csvConfiguration);
            var fileAnalysis = new FileAnalysisDto();
            var rowIndex = 0;
            var MaxRows = 11;
            var records = new string[MaxRows][];

            while (csv.Read())
            {
                if (rowIndex >= MaxRows) break;

                records[rowIndex] = new string[csv.Parser.Count];

                for (int i = 0; csv.TryGetField<string>(i, out var value) && value != null; i++)
                {
                    records[rowIndex][i] = value;
                }

                rowIndex++;
            }

            if (rowIndex == 0) return BadRequest("No rows detected");

            var header = records[0];

            for (int j = 0; j < header.Length; j++)
            {
                var columnSetting = new ColumnSettingDto
                {
                    ColumnName = header[j],
                    Include = true
                };

                var counters = new[] { 0, 0, 0, 0 };
                for (int i = 1; i < records.Length; i++)
                {
                    //datetime, int, decimal, ,string
                    var value = records[i]?[j];

                    if (int.TryParse(value, out var valueAsInt))
                    {
                        counters[1]++;
                    }
                    else if (decimal.TryParse(value, out var valueAsDecimal))
                    {
                        counters[2]++;
                    }
                    else if (DateTime.TryParse(value, out var valueAsDateTime))
                    {
                        counters[0]++;
                    }
                    else
                    {
                        counters[3]++;
                    }
                }

                int maxValue = counters.Max();
                int maxIndex = counters.ToList().IndexOf(maxValue);

                switch (maxIndex)
                {
                    case 0: columnSetting.Type = ColumnDataTypeEnum.Datetime.ToString(); break;
                    case 1: //columnSetting.Type = ColumnDataTypeEnum.Int.ToString(); break;
                    case 2: columnSetting.Type = ColumnDataTypeEnum.Decimal.ToString(); break;
                    case 3: columnSetting.Type = ColumnDataTypeEnum.String.ToString(); break;
                }

                fileAnalysis.ColumnsSettings.Add(columnSetting);
            }

            fileAnalysis.Header = fileAnalysis.ColumnsSettings.Select(x => x.ColumnName).ToArray();
            fileAnalysis.Data = records;

            return Ok(fileAnalysis);
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

                var MaxRows = 11;
                var preview = data.Preview(maxRows: MaxRows);
                var rowIndex = 0;

                var records = new string[MaxRows][];

                foreach (var row in preview.RowView)
                {
                    records[rowIndex] = new string[row.Values.Length];
                    var values = row.Values.Select(x => x.Value).ToArray();
                    var ColumnCollection = row.Values;

                    for (int i = 0; i < row.Values.Length; i++)
                    {
                        records[rowIndex][i] = values[i]?.ToString() ?? "";
                    }
                    rowIndex++;
                }

                fileAnalysis.Data = records;

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

        private void PP()
        {
            var ds = _dbContext.Datasets.Find(7);
            _dbContext.Entry(ds).Reference(nameof(ds.FileStorage)).Load();

            var file = ds.FileStorage;

            using var memStream = new MemoryStream(file.Data);
            using var tr = new StreamReader(memStream);
            var csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture);
            using var csv = new CsvReader(tr, csvConfiguration);
            var readed = csv.Read();
        }
    }
}