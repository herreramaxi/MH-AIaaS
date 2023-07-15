using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Application.Specifications;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Entities.enums;
using AIaaS.Domain.Interfaces;
using AIaaS.WebAPI.ExtensionMethods;
using AIaaS.WebAPI.Interfaces;
using Microsoft.ML;
using System.Text.Json;

namespace AIaaS.Application.Common.Models.Operators
{
    [Operator("Dataset", OperatorType.Dataset, 1)]
    [OperatorParameter("Dataset", "A dataset is a data source or input file for training a machine learning model", "list")]
    [OperatorParameter("SelectedColumns", "Selected columns to be included on model training", "list")]
    public class DatasetOperator : WorkflowOperatorAbstract
    {
        private IList<string>? _selectedColumns;
        private int? _datasetId;
        private readonly IReadRepository<Dataset> _repository;

        public DatasetOperator(IReadRepository<Dataset> repository, IWorkflowService workflowService) : base(workflowService)
        {
            _repository = repository;
        }

        public override Task Hydrate(WorkflowContext mlContext, WorkflowNodeDto root)
        {
            _datasetId = root.GetParameterValue<int>("Dataset");
            _selectedColumns = root.GetParameterValue<string, IList<string>>("SelectedColumns", x => JsonSerializer.Deserialize<IList<string>>(x));
            return Task.CompletedTask;
        }

        public override bool Validate(WorkflowContext mlContext, WorkflowNodeDto root)
        {
            if (_datasetId is null)
            {
                root.SetAsFailed("Please select a dataset");
                return false;
            }

            if (_selectedColumns is null || !_selectedColumns.Any())
            {
                root.SetAsFailed("Please select columns from dataset");
                return false;
            }

            return true;
        }

        public override async Task Run(WorkflowContext context, WorkflowNodeDto root, CancellationToken cancellationToken)
        {
            if (_datasetId is null || _selectedColumns is null || !_selectedColumns.Any())
                return;

            context.Dataset = await _repository.FirstOrDefaultAsync(new DatasetByIdWithColumnSettingsAndDataViewFileSpec(_datasetId.Value));

            if (context.Dataset is null)
            {
                root.SetAsFailed("Dataset not found");
                return;
            }

            if (context.Dataset.DataViewFile?.Data is null)
            {
                root.SetAsFailed("DataViewFile not found or its data is empty");
                return;
            }

            if (context.Dataset.ColumnSettings is null || !context.Dataset.ColumnSettings.Any())
            {
                root.SetAsFailed("ColumnsSettings not found");
                return;
            }

            var datasetColumnNames = context.Dataset.ColumnSettings.Select(x => x.ColumnName);
            var nonExistingColumnNames = _selectedColumns.Where(x => !datasetColumnNames.Contains(x, StringComparer.InvariantCultureIgnoreCase));

            if (nonExistingColumnNames.Any())
            {
                root.SetAsFailed($"The following selected columns do not exists in Dataset: {string.Join(", ", nonExistingColumnNames)}");
                return;
            }

            //TODO: propagate this columns, so if I add editDataset operator, then it will modify and propagate those columns
            context.ColumnSettings = context.Dataset.ColumnSettings.Where(x => _selectedColumns.Contains(x.ColumnName, StringComparer.InvariantCultureIgnoreCase));
            var columnsToBeDropped = context.Dataset.ColumnSettings.Where(x => !_selectedColumns.Contains(x.ColumnName, StringComparer.InvariantCultureIgnoreCase))
                .Select(x => x.ColumnName)
                .ToArray();

            //TODO: check how to manage usings cos if I dispose IDataview cannot be processed
            var memStream = new MemoryStream(context.Dataset.DataViewFile.Data);
            var mss = new MultiStreamSourceFile(memStream);
            var mlContext = new MLContext();
            context.DataView = mlContext.Data.LoadFromBinary(mss);
            context.InputOutputColumns = context.ColumnSettings.Select(x => new InputOutputColumnPair(x.ColumnName, x.ColumnName)).ToArray();

            if (columnsToBeDropped.Any())
            {
                var estimator = context.MLContext.Transforms.DropColumns(columnsToBeDropped);
                context.EstimatorChain = context.EstimatorChain.AppendEstimator(estimator);
            }
        }

        public override void PropagateDatasetColumns(WorkflowContext context, WorkflowNodeDto root)
        {
            root.SetDatasetColumns(_selectedColumns);
        }
    }
}