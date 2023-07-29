using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Domain.Entities.enums;
using AIaaS.WebAPI.Services;
using Ardalis.Result;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Text.Json;

namespace AIaaS.Application.Features.Workflows.Commands.Common.Operators
{
    [Operator("Evaluate", OperatorType.Evaluate, 5)]
    //[OperatorParameter("Label", "The name of the label column", "text")]
    public class EvaluateModelOperator : WorkflowOperatorAbstract
    {
        private readonly ILogger<EvaluateModelOperator> _logger;

        public EvaluateModelOperator(ILogger<EvaluateModelOperator> logger, IOperatorService operatorService) : base(operatorService)
        {
            _logger = logger;
        }

        public override Task Hydrate(WorkflowContext context, WorkflowNodeDto root)
        {
            return Task.CompletedTask;
        }

        public override Result Validate(WorkflowContext context, WorkflowNodeDto root)
        {
            if (root.Data?.DatasetColumns is null || !root.Data.DatasetColumns.Any())
            {
                return Result.Error("No selected columns detected on pipeline, please select columns on dataset operator");
            }

            return Result.Success();
        }

        public override async Task<Result> Run(WorkflowContext context, WorkflowNodeDto root, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(context.LabelColumn))
            {
                return Result.Error("Label column is not found, please add a 'Train Model' operator into the pipeline and select a label column");
            }

            if (context.Trainer is null)
            {
                return Result.Error("Trainer not found, please add a 'Train Model' operator into the pipeline");
            }

            if (context.TestData is null)
            {
                return Result.Error("Test data not found, please add a 'Split Data' operator into the pipeline");
            }

            if (context.Task is null)
            {
                return Result.Error($"ML Task not found, please ensure the 'Train Model' operator is correctly configured");
            }

            IDataView predictions = context.TrainedModel.Transform(context.TestData);

            //TODO: Configure score column
            if (context.Task == MetricTypeEnum.Regression)
            {
                var metrics = context.MLContext.Regression.Evaluate(predictions, labelColumnName: context.LabelColumn, scoreColumnName: "Score");
                var modelMetrics = new RegressionMetricsDto
                {
                    Task = context.Task.ToString(),
                    LossFunction = metrics.LossFunction.ToString(),
                    MeanAbsoluteError = metrics.MeanAbsoluteError.ToString(),
                    MeanSquaredError = metrics.MeanSquaredError.ToString(),
                    RootMeanSquaredError = metrics.RootMeanSquaredError.ToString(),
                    RSquared = metrics.RSquared.ToString()
                };
                context.MetricsSerialized = JsonSerializer.Serialize(modelMetrics);

                PrintRegressionMetrics(context.Trainer?.ToString() ?? "N/A", metrics);
            }
            else if (context.Task == MetricTypeEnum.BinaryClassification)
            {
                var metrics = context.MLContext.BinaryClassification.Evaluate(predictions, labelColumnName: context.LabelColumn, scoreColumnName: "Score");
                var modelMetrics = new BinaryClasifficationMetricsDto
                {
                    Task = context.Task.ToString(),
                    LogLossReduction = metrics.LogLossReduction.ToString(),
                    Accuracy = metrics.Accuracy.ToString(),
                    LogLoss = metrics.LogLoss.ToString(),
                    NegativeRecall = metrics.NegativeRecall.ToString(),
                    PositiveRecall = metrics.PositiveRecall.ToString(),
                    AreaUnderPrecisionRecallCurve = metrics.AreaUnderPrecisionRecallCurve.ToString(),
                    AreaUnderRocCurve = metrics.AreaUnderRocCurve.ToString(),
                    Entropy = metrics.Entropy.ToString(),
                    F1Score = metrics.F1Score.ToString()
                };
                context.MetricsSerialized = JsonSerializer.Serialize(modelMetrics);
            }
            else
            {
                return Result.Error($"Evaluate operator is not able to generate metrics for task: {context.Task}");
            }

            if (context.Workflow.MLModel is null)
            {
                return Result.Error("Model not found, please add a 'Train Model' operator into the pipeline");
            }

            return Result.Success();
        }

        public void PrintRegressionMetrics(string name, RegressionMetrics metrics)
        {
            _logger.LogInformation($"*************************************************");
            _logger.LogInformation($"*       Metrics for {name} regression model      ");
            _logger.LogInformation($"*------------------------------------------------");
            _logger.LogInformation($"*       LossFn:        {metrics.LossFunction:0.##}");
            _logger.LogInformation($"*       R2 Score:      {metrics.RSquared:0.##}");
            _logger.LogInformation($"*       Absolute loss: {metrics.MeanAbsoluteError:#.##}");
            _logger.LogInformation($"*       Squared loss:  {metrics.MeanSquaredError:#.##}");
            _logger.LogInformation($"*       RMS loss:      {metrics.RootMeanSquaredError:#.##}");
            _logger.LogInformation($"*************************************************");
        }

        override public async Task GenerateOuput(WorkflowContext context, WorkflowNodeDto root, CancellationToken cancellationToken)
        {
            if (context.Workflow.MLModel is null || string.IsNullOrEmpty(context.MetricsSerialized)) return;

            await _operatorService.UpdateModelMetrics(context.Workflow, context.Task.Value, context.MetricsSerialized, cancellationToken);
            root.Data.Parameters = new Dictionary<string, object>();
            root.Data.Parameters.Add("ModelMetricsId", context.Workflow.MLModel.ModelMetrics.Id);
        }
    }
}
