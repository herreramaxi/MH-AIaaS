using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Models;
using AIaaS.WebAPI.Models.Dtos;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public DatasetsController(EfContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var datasets = await _dbContext.Datasets.ToListAsync();
            var dtos = datasets.Select(x => new DatasetDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                CreatedBy = x.CreatedBy,
                CreatedOn = x.CreatedOn,
                ModifiedBy = x.ModifiedBy,
                ModifiedOn = x.ModifiedOn,
                ColumnSettings = x.ColumnSettings.Select(x => new ColumnSettingDto
                {
                    ColumnName = x.ColumnName,
                    Id = x.Id,
                    Include = x.Include,
                    Type = x.Type
                }).ToList()
            });
            return Ok(dtos);
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

        [HttpPost("Upload/{datasetId}")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, int datasetId)
        {
            if (file is null)
                return BadRequest("File is required");

            var dataset = await _dbContext.Datasets.FindAsync(datasetId);

            if (dataset is null)
                return BadRequest("Dataset not found");

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

            return Created("", null);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Remove([FromRoute]int id)
        {
            if (id<=0)
                return BadRequest("Id parameter should be greater than zero");

            var dataset =await  _dbContext.Datasets.FirstOrDefaultAsync(x => x.Id == id);
            if (dataset is null)
                return NotFound();

            _dbContext.Datasets.Remove(dataset);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("Preview")]
        public IActionResult Preview([FromForm] IFormFile file)
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
                    var value = records[i][j];

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
                    case 0: columnSetting.Type = "Datetime"; break;
                    case 1: columnSetting.Type = "Int"; break;
                    case 2: columnSetting.Type = "Decimal"; break;
                    case 3: columnSetting.Type = "String"; break;
                }

                fileAnalysis.ColumnsSettings.Add(columnSetting);
            }

            fileAnalysis.Header = fileAnalysis.ColumnsSettings.Select(x => x.ColumnName).ToArray();
            fileAnalysis.Data = records;

            return Ok(fileAnalysis);
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