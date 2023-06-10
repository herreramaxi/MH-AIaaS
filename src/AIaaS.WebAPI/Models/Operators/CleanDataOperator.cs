using AIaaS.WebAPI.Models.Dtos;
using AIaaS.WebAPI.Models.enums;
using Microsoft.ML;

namespace AIaaS.WebAPI.Models.Operators
{
    [Operator("Clean Data", OperatorType.Clean, 2)]
    [OperatorParameter("Cleaning mode", "Cleaning mode to be applied", "list")]
    [OperatorParameter("SelectedColumns", "Columns to be cleaned", "list")]
    public class CleanDataOperator : WorkflowOperatorAbstract
    {
        private readonly ILogger<CleanDataOperator> _logger;

        public CleanDataOperator(ILogger<CleanDataOperator> logger)
        {
            _logger = logger;
        }
        public override Task Hydrate(WorkflowContext mlContext, WorkflowNodeDto root)
        {
            return Task.CompletedTask;
        }

        public override bool Validate(WorkflowContext context, WorkflowNodeDto root)
        {
            if (root.Data?.DatasetColumns is null || !root.Data.DatasetColumns.Any())
            {
                root.SetAsFailed("No selected columns detected on pipeline, please select columns on dataset operator");
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

            var mlContext = context.MLContext;
            var estimator = mlContext.Transforms.ReplaceMissingValues(context.InputOutputColumns, Microsoft.ML.Transforms.MissingValueReplacingEstimator.ReplacementMode.Mean);

            context.EstimatorChain = context.EstimatorChain is not null ?
                context.EstimatorChain.Append(estimator) :
                estimator;

            //var transformer = context.EstimatorChain.Fit(context.DataView);
            //var dataview = transformer.Transform(context.DataView);
            //var preview = dataview.Preview(50);

            return Task.CompletedTask;
        }
    }
}

