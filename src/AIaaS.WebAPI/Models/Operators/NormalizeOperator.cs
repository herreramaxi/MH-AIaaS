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
        public override async Task Execute(WorkflowContext context, WorkflowNodeDto root)
        {
            if (context.InputOutputColumns is null || !context.InputOutputColumns.Any())
            {
                root.Error("No selected columns detected on pipeline, please select columns on dataset operator");
                return;
            }

            var mlContext = context.MLContext;
            var estimator = mlContext.Transforms.NormalizeMeanVariance(context.InputOutputColumns);

            context.EstimatorChain = context.EstimatorChain is not null ?
                context.EstimatorChain.Append(estimator) :
                estimator;

            root.Success();
        }
    }
}
