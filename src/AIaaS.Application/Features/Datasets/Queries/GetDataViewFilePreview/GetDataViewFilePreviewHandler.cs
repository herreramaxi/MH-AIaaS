using AIaaS.Application.Common.Models;
using AIaaS.Application.Specifications.Datasets;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.ML;

namespace AIaaS.Application.Features.Datasets.Queries.GetDataViewFilePreview
{
    public class GetDataViewFilePreviewHandler : IRequestHandler<GetDataViewFilePreviewRequest, DataViewFilePreviewDto?>
    {
        private readonly IReadRepository<Dataset> _datasetRepository;
        private readonly IMapper _mapper;

        public GetDataViewFilePreviewHandler(IReadRepository<Dataset> datasetRepository, IMapper mapper)
        {
            _datasetRepository = datasetRepository;
            _mapper = mapper;
        }

        public async Task<DataViewFilePreviewDto?> Handle(GetDataViewFilePreviewRequest request, CancellationToken cancellationToken)
        {
            var dataset = await _datasetRepository.FirstOrDefaultAsync(new DatasetByIdWithDataViewFileSpec(request.DatasetId), cancellationToken);
            if (dataset?.DataViewFile is null) return null;

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

            var dataPreview = new DataViewFilePreviewDto
            {
                Header = header,
                Rows = records
            };

            return dataPreview;
        }
    }
}
