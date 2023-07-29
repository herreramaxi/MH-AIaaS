using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Domain.Entities.enums;
using AIaaS.WebAPI.Interfaces;
using AIaaS.WebAPI.Services;
using Ardalis.Result;

namespace AIaaS.Application.Features.Workflows.Commands.Common.Operators
{
    [Operator("Split Data", OperatorType.Split, 3)]
    [OperatorParameter("Test Fraction", "The fraction of data to go into the test set", "text", "0.2")]
    public class SplitDataOperator : WorkflowOperatorAbstract
    {
        private double _fraction;

        public SplitDataOperator(IOperatorService operatorService) : base(operatorService)
        {
        }

        public override Task Hydrate(WorkflowContext mlContext, WorkflowNodeDto root)
        {
            return Task.CompletedTask;
        }

        public override Result Validate(WorkflowContext mlContext, WorkflowNodeDto root)
        {
            if (root.Data?.DatasetColumns is null || !root.Data.DatasetColumns.Any())
            {
              return Result.Error("No selected columns detected on pipeline, please select columns on dataset operator");
            }

            var fraction = root.GetParameterValue<double>("Test Fraction");

            if (fraction is null || fraction == 0)
            {
                return Result.Error("Please enter a 'Test Fraction' greater than zero");
            }

            _fraction = (double)fraction;

            return Result.Success();
        }

        public override async Task<Result> Run(WorkflowContext context, WorkflowNodeDto root, CancellationToken cancellationToken)
        {
            var mlContext = context.MLContext;

            if (context.DataView is null)
            {
                return Result.Error("DataView not found, please ensure there is a 'dataset' operator correctly configured");
            }

            var trainTestSplit = mlContext.Data.TrainTestSplit(context.DataView, testFraction: _fraction);
            context.TrainingData = trainTestSplit.TrainSet;
            context.TestData = trainTestSplit.TestSet;

            return Result.Success();
        }
    }
}
