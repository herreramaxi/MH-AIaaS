using AIaaS.WebAPI.Models.Dtos;
using AIaaS.WebAPI.Models.enums;

namespace AIaaS.WebAPI.Models.Operators
{
    [Operator("Split Data", OperatorType.Split, 3)]
    [OperatorParameter("Test Fraction", "The fraction of data to go into the test set", "text", "0.2")]
    public class SplitDataOperator : WorkflowOperatorAbstract
    {
        private double _fraction;

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

            var fraction = root.GetParameterValue<double>("Test Fraction");

            if (fraction is null || fraction == 0)
            {
                root.SetAsFailed("Please enter a 'Test Fraction' greater than zero");
                return false;
            }

            _fraction = (double)fraction;

            return true;
        }

        public override Task Run(WorkflowContext context, WorkflowNodeDto root)
        {
            var mlContext = context.MLContext;

            if (context.DataView is null)
            {
                root.SetAsFailed("DataView not found, please ensure there is a 'dataset' operator correctly configured");
                return Task.CompletedTask;
            }

            var trainTestSplit = mlContext.Data.TrainTestSplit(context.DataView, testFraction: _fraction);
            context.TrainingData = trainTestSplit.TrainSet;
            context.TestData = trainTestSplit.TestSet;

            return Task.CompletedTask;
        }
    }
}
