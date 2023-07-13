using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Domain.Entities.enums;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Text.Json;

namespace AIaaS.Application.Common.Models.Operators
{
    [Operator("Edit Dataset", OperatorType.EditDataset, 6)]
    [OperatorParameter("DataType", "Selected columns to be converted", "list")]
    [OperatorParameter("Categorical", "Selected columns to be converted", "list")]
    [OperatorParameter("SelectedColumns", "Selected columns to be converted", "list")]
    public class EditDatasetOPerator : WorkflowOperatorAbstract
    {
        private string? _dataType;
        private string? _categorical;
        private IList<string>? _selectedColumns;

        public override Task Hydrate(WorkflowContext context, WorkflowNodeDto root)
        {
            _dataType = root.GetParameterValue("DataType");
            _categorical = root.GetParameterValue("Categorical");
            _selectedColumns = root.GetParameterValue<string, IList<string>>("SelectedColumns", x => JsonSerializer.Deserialize<IList<string>>(x));
            return Task.CompletedTask;
        }

        public override bool Validate(WorkflowContext context, WorkflowNodeDto root)
        {
            if (_selectedColumns is null || !_selectedColumns.Any())
            {
                root.SetAsFailed("Please select columns to be converted");
                return false;
            }

            if (string.IsNullOrEmpty(_dataType) && string.IsNullOrEmpty(_categorical))
            {
                root.SetAsFailed("At least you must select a data type categorical conversion");
                return false;
            }

            return true;
        }

        public override Task Run(WorkflowContext context, WorkflowNodeDto root)
        {
            if (_selectedColumns is null || !_selectedColumns.Any() || context.InputOutputColumns is null || !context.InputOutputColumns.Any())
            {
                root.SetAsFailed("Please ensure the operator is correctly configured");
                return Task.CompletedTask;
            }

            var mlContext = context.MLContext;
            var columnsToBeConverted = context.InputOutputColumns.Where(x => _selectedColumns.Contains(x.InputColumnName, StringComparer.InvariantCultureIgnoreCase)).ToArray();

            if (!string.IsNullOrEmpty(_dataType))
            {
                var converted = Enum.TryParse<DataKind>(_dataType, out var dataKind);

                if (!converted)
                {
                    root.SetAsFailed("Wrong value from data type");
                    return Task.CompletedTask;
                }

                var estimator = mlContext.Transforms.Conversion.ConvertType(columnsToBeConverted, dataKind);

                context.EstimatorChain = context.EstimatorChain is not null ?
                    context.EstimatorChain.Append(estimator) :
                    estimator;
            }

            if (!string.IsNullOrEmpty(_categorical))
            {
                var estimator = GetCategoricalEstimator(mlContext, columnsToBeConverted);

                if (estimator is null)
                {
                    root.SetAsFailed("Wrong value from categorical type");
                    return Task.CompletedTask;
                }

                context.EstimatorChain = context.EstimatorChain is not null ?
                    context.EstimatorChain.Append(estimator) :
                    estimator;
            }
         
            return Task.CompletedTask;
        }

        private IEstimator<ITransformer>? GetCategoricalEstimator(MLContext mlContext, InputOutputColumnPair[] columnsToBeConverted)
        {
            switch (_categorical)
            {
                case "OneHotEncoding": return mlContext.Transforms.Categorical.OneHotEncoding(columnsToBeConverted);
                case "OneHotHashEncoding": return mlContext.Transforms.Categorical.OneHotHashEncoding(columnsToBeConverted);
                default:
                    return null;
            }
        }
    }
}
