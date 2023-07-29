using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Application.Interfaces;
using AIaaS.Application.Services;
using AIaaS.Application.Specifications;
using AIaaS.Application.Specifications.Datasets;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Entities.enums;
using AIaaS.Domain.Interfaces;
using AIaaS.WebAPI.ExtensionMethods;
using AIaaS.WebAPI.Interfaces;
using AIaaS.WebAPI.Services;
using Ardalis.Result;
using Microsoft.ML;
using System.Text.Json;

namespace AIaaS.Application.Features.Workflows.Commands.Common.Operators
{
    [Operator("Dataset", OperatorType.Dataset, 1)]
    [OperatorParameter("Dataset", "A dataset is a data source or input file for training a machine learning model", "list")]
    [OperatorParameter("SelectedColumns", "Selected columns to be included on model training", "list")]
    public class DatasetOperator : WorkflowOperatorAbstract
    {
        private IList<string>? _selectedColumns;
        private int? _datasetId;
        private readonly IReadRepository<Dataset> _repository;
        private readonly IDataViewService _dataViewService;

        public DatasetOperator(IReadRepository<Dataset> repository, IOperatorService operatorService, IDataViewService dataViewService) : base(operatorService)
        {
            _repository = repository;
            _dataViewService = dataViewService;
        }

        public override Task Hydrate(WorkflowContext mlContext, WorkflowNodeDto root)
        {
            _datasetId = root.GetParameterValue<int>("Dataset");
            _selectedColumns = root.GetParameterValue<string, IList<string>>("SelectedColumns", x => JsonSerializer.Deserialize<IList<string>>(x));
            return Task.CompletedTask;
        }

        public override Result Validate(WorkflowContext mlContext, WorkflowNodeDto root)
        {
            if (_datasetId is null)
            {
                return Result.Error("Please select a dataset");
            }

            if (_selectedColumns is null || !_selectedColumns.Any())
            {
                return Result.Error("Please select columns from dataset");
            }

            return Result.Success();
        }

        public override async Task<Result> Run(WorkflowContext context, WorkflowNodeDto root, CancellationToken cancellationToken)
        {
            if (_datasetId is null || _selectedColumns is null || !_selectedColumns.Any())
                return Result.Error("Parameters not provided");

            context.Dataset = await _repository.FirstOrDefaultAsync(new DatasetByIdWithColumnSettingsAndDataViewFileSpec(_datasetId.Value));

            if (context.Dataset is null)
            {
                return Result.Error("Dataset not found");
            }

            if (context.Dataset.DataViewFile is null)
            {
                return Result.Error("DataViewFile not found");
            }

            if (context.Dataset.ColumnSettings is null || !context.Dataset.ColumnSettings.Any())
            {
                return Result.Error("ColumnsSettings not found");
            }

            var datasetColumnNames = context.Dataset.ColumnSettings.Select(x => x.ColumnName);
            var nonExistingColumnNames = _selectedColumns.Where(x => !datasetColumnNames.Contains(x, StringComparer.InvariantCultureIgnoreCase));

            if (nonExistingColumnNames.Any())
            {
                return Result.Error($"The following selected columns do not exists in Dataset: {string.Join(", ", nonExistingColumnNames)}");
            }

            //TODO: propagate this columns, so if I add editDataset operator, then it will modify and propagate those columns
            context.ColumnSettings = context.Dataset.ColumnSettings.Where(x => _selectedColumns.Contains(x.ColumnName, StringComparer.InvariantCultureIgnoreCase));
            var columnsToBeDropped = context.Dataset.ColumnSettings.Where(x => !_selectedColumns.Contains(x.ColumnName, StringComparer.InvariantCultureIgnoreCase))
                .Select(x => x.ColumnName)
                .ToArray();

            var dataViewResult = await _dataViewService.GetDataViewAsync(context.Dataset.DataViewFile);
            if (!dataViewResult.IsSuccess)
            {
                return Result.Error(dataViewResult.Errors.FirstOrDefault() ?? "Error wehn trying to downloaddataview file from S3");
            }

            context.DataView = dataViewResult.Value;
            context.InputOutputColumns = context.ColumnSettings.Select(x => new InputOutputColumnPair(x.ColumnName, x.ColumnName)).ToArray();

            if (columnsToBeDropped.Any())
            {
                var estimator = context.MLContext.Transforms.DropColumns(columnsToBeDropped);
                context.EstimatorChain = context.EstimatorChain.AppendEstimator(estimator);
            }

            return Result.Success();
        }

        public override void PropagateDatasetColumns(WorkflowContext context, WorkflowNodeDto root)
        {
            root.SetDatasetColumns(_selectedColumns);
        }
    }
}