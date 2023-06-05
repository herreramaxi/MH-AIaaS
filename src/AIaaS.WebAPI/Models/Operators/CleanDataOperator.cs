using AIaaS.WebAPI.Models.enums;
using Microsoft.ML;

namespace AIaaS.WebAPI.Models.Operators
{
    [Operator("Clean Data", OperatorType.Clean, 2)]
    [OperatorParameter("Variable Name", "The name of the new or existing variable", "text")]
    [OperatorParameter("Value", "Javascript expression for the value", "text")]
    public class CleanDataOperator : WorkflowOperatorAbstract
    {
        private readonly ILogger<CleanDataOperator> _logger;

        public CleanDataOperator(ILogger<CleanDataOperator> logger)
        {
            _logger = logger;
        }
        public override async Task Execute(WorkflowContext context, Dtos.WorkflowNodeDto root)
        {
            if (context.InputOutputColumns is null || !context.InputOutputColumns.Any())
            {
                root.Error("No selected columns detected on pipeline, please select columns on dataset operator");
                return;
            }

            var mlContext = context.MLContext;
            var estimator = mlContext.Transforms.ReplaceMissingValues(context.InputOutputColumns, Microsoft.ML.Transforms.MissingValueReplacingEstimator.ReplacementMode.Mean);

            context.EstimatorChain = context.EstimatorChain is not null ?
                context.EstimatorChain.Append(estimator) :
                estimator;

            //var transformer = context.EstimatorChain.Fit(context.DataView);
            //var dataview = transformer.Transform(context.DataView);
            //var preview = dataview.Preview(50);

            root.Success();
        }     
    }
}
