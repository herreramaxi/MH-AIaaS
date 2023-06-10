using AIaaS.WebAPI.Models.Dtos;
using AIaaS.WebAPI.Models.enums;
using Microsoft.ML;

namespace AIaaS.WebAPI.Models.Operators
{
    [Operator("Normalize", OperatorType.Normalize, 7)]
    [OperatorParameter("Variable Name", "The name of the new or existing variable", "text")]
    [OperatorParameter("Value", "Javascript expression for the value", "text")]
    public class NormalizeOperator : WorkflowOperatorAbstract
    {
        public override Task Hydrate(WorkflowContext mlContext, WorkflowNodeDto root)
        {
            return Task.CompletedTask;
        }

        public override bool Validate(WorkflowContext mlContext, WorkflowNodeDto root)
        {
            if (root.Data?.DatasetColumns is null || !root.Data.DatasetColumns.Any())
            {
                root.SetAsFailed("No selected columns detected on pipeline, please select columns on dataset operator");
                return false;
            }

            return true;
        }

        public override Task Run(WorkflowContext context, WorkflowNodeDto root)
        {
            if (context.InputOutputColumns is null || !context.InputOutputColumns.Any())
            {
                root.SetAsFailed("No selected columns detected on pipeline, please select columns on dataset operator");
                return Task.CompletedTask;
            }

            var mlContext = context.MLContext;
            var estimator = mlContext.Transforms.NormalizeMeanVariance(context.InputOutputColumns);

            context.EstimatorChain = context.EstimatorChain is not null ?
                context.EstimatorChain.Append(estimator) :
                estimator;

            return Task.CompletedTask;
        }
    }
}
