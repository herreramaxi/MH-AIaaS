using AIaaS.WebAPI.Models.Dtos;
using AIaaS.WebAPI.Models.enums;

namespace AIaaS.WebAPI.Models.Operators
{
    [Operator("Split Data", OperatorType.Split, 3)]
    [OperatorParameter("Test Fraction", "The fraction of data to go into the test set", "text")] 
    public class SplitDataOperator : WorkflowOperatorAbstract
    {
        public override Task Execute(WorkflowContext context, WorkflowNodeDto root)
        {
            var mlContext = context.MLContext;
            
            var fraction = root.GetParameterValue<double>("Test Fraction");

            if (fraction is null || fraction == 0) {
                root.Error("Please enter a 'Test Fraction' greater than zero");
                return Task.CompletedTask;
            }

            var trainTestSplit = mlContext.Data.TrainTestSplit(context.DataView, testFraction: (double)fraction);
            context.TrainingData = trainTestSplit.TrainSet;
            context.TestData = trainTestSplit.TestSet;
            root.Success();
            return Task.CompletedTask;
        }
    }
}
