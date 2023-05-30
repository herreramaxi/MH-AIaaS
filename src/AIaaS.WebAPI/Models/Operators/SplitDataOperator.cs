using AIaaS.WebAPI.Models.enums;

namespace AIaaS.WebAPI.Models.Operators
{
    [Operator("Split Data", OperatorType.Split, 3)]
    [OperatorParameter("Variable Name", "The name of the new or existing variable", "text")]
    [OperatorParameter("Value", "Javascript expression for the value", "text")]
    public class SplitDataOperator : WorkflowOperatorAbstract
    {
        public override Task Execute(WorkflowContext context, Dtos.WorkflowNodeDto root)
        {
            var mlContext = context.MLContext;
            var trainTestSplit = mlContext.Data.TrainTestSplit(context.DataView, testFraction: 0.2);
            context.TrainingData = trainTestSplit.TrainSet;
            context.TestData = trainTestSplit.TestSet;

            return Task.CompletedTask;
        }
    }
}
