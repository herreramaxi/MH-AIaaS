using AIaaS.WebAPI.CQRS.Queries;
using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Models;
using Ardalis.Result;
using AutoMapper;
using MediatR;
using Microsoft.ML;

namespace AIaaS.WebAPI.CQRS.Handlers
{
    public class GetPreviewWorkflowHandler : IRequestHandler<GetPreviewWorkflowQuery, Result<object>>
    {
        private readonly EfContext _dbContext;

        public GetPreviewWorkflowHandler(EfContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Result<object>> Handle(GetPreviewWorkflowQuery request, CancellationToken cancellationToken)
        {
            var dataView = await _dbContext.WorkflowDataViews.FindAsync(request.WorkflowDataviewId);
            if (dataView?.Data is null) return Result.NotFound();

            using var memStream = new MemoryStream(dataView.Data);
            var mss = new MultiStreamSourceFile(memStream);
            var mlContext = new MLContext();
            var dataview = mlContext.Data.LoadFromBinary(mss);
            var header = dataview.Schema.Select(x => x.Name);
            var MaxRows = 100;
            var preview = dataview.Preview(maxRows: MaxRows);

            var records = new List<string[]>();
            var columns = preview.Schema.Where(x => !x.IsHidden).Select(x => new { x.Index, x.Name });
            var columnIndices = columns.Select(x => x.Index).ToHashSet();

            foreach (var row in preview.RowView)
            {
                var record = row.Values
                    .Where((x, i) => columnIndices.Contains(i))
                    .Select(x => x.Value?.ToString() ?? "")
                    .ToArray();

                records.Add(record);
            }

            object objectResult = new
            {
                header,
                rows = records
            };

            return Result.Success(objectResult);
        }
    }
}
