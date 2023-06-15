using AIaaS.WebAPI.Models.Dtos;
using AIaaS.WebAPI.Models.enums;
using Microsoft.ML;
using System.Text.Json;

namespace AIaaS.WebAPI.Models.Operators
{
    [Operator("Clean Data", OperatorType.Clean, 2)]
    [OperatorParameter("Cleaning mode", "Cleaning mode to be applied", "list")]
    [OperatorParameter("SelectedColumns", "Columns to be cleaned", "list")]
    public class CleanDataOperator : WorkflowOperatorAbstract
    {
        private readonly ILogger<CleanDataOperator> _logger;
        private string? _cleanMode;
        private IList<string>? _selectedColumns;

        public CleanDataOperator(ILogger<CleanDataOperator> logger)
        {
            _logger = logger;
        }
        public override Task Hydrate(WorkflowContext mlContext, WorkflowNodeDto root)
        {
            _cleanMode = root.GetParameterValue("Cleaning mode");
            _selectedColumns = root.GetParameterValue<string, IList<string>>("SelectedColumns", x => JsonSerializer.Deserialize<IList<string>>(x));

            return Task.CompletedTask;
        }

        public override bool Validate(WorkflowContext context, WorkflowNodeDto root)
        {
            if (string.IsNullOrEmpty(_cleanMode))
            {
                root.SetAsFailed("Clean mode not selected, please select a clean mode");
                return false;
            }

            if (root.Data?.DatasetColumns is null || !root.Data.DatasetColumns.Any())
            {
                root.SetAsFailed("No selected columns detected on pipeline, please select columns on dataset operator");
                return false;
            }

            if (_selectedColumns is null || !_selectedColumns.Any())
            {
                root.SetAsFailed("No selected columns detected on operator, please select any column to be cleaned");
                return false;
            }

            return true;
        }

        public override Task Run(WorkflowContext context, Dtos.WorkflowNodeDto root)
        {
            if (context.InputOutputColumns is null || !context.InputOutputColumns.Any())
            {
                root.SetAsFailed("No selected columns detected on pipeline, please select columns on dataset operator");
                return Task.CompletedTask;
            }

            if (_cleanMode is null || _selectedColumns is null || !_selectedColumns.Any())
            {
                root.SetAsFailed("Please verify the operator is correctly configured");
                return Task.CompletedTask;
            }

            var mlContext = context.MLContext;
            var selectedColumns = context.InputOutputColumns
                .Where(x => _selectedColumns.Contains(x.InputColumnName, StringComparer.InvariantCultureIgnoreCase))
                .ToArray();

            if (_cleanMode.Equals("RemoveRow"))
            {
                //TODO:
            }
            else
            {
                Enum.TryParse<Microsoft.ML.Transforms.MissingValueReplacingEstimator.ReplacementMode>(_cleanMode, out var replacementMode);
                var estimator = mlContext.Transforms.ReplaceMissingValues(selectedColumns, replacementMode);
                context.EstimatorChain = context.EstimatorChain is not null ?
                    context.EstimatorChain.Append(estimator) :
                    estimator;
            }


            //var transformer = context.EstimatorChain.Fit(context.DataView);
            //var dataview = transformer.Transform(context.DataView);
            //var preview = dataview.Preview(50);

            return Task.CompletedTask;
        }
    }
}

