using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Models.Dtos;
using AIaaS.WebAPI.Models.enums;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Text.Json;

namespace AIaaS.WebAPI.Models.Operators
{
    [Operator("Evaluate", OperatorType.Evaluate, 5)]
    //[OperatorParameter("Label", "The name of the label column", "text")]
    public class EvaluateModelOperator : WorkflowOperatorAbstract
    {
        private readonly ILogger<EvaluateModelOperator> _logger;
        private readonly EfContext _dbContext;

        public EvaluateModelOperator(ILogger<EvaluateModelOperator> logger, EfContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public override Task Hydrate(WorkflowContext context, WorkflowNodeDto root)
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

        public override async Task Run(WorkflowContext context, WorkflowNodeDto root)
        {         
            if (string.IsNullOrEmpty(context.LabelColumn))
            {
                root.SetAsFailed("Label column is not found, please add a 'Train Model' operator into the pipeline and select a label column");
                return;
            }

            if (context.Trainer is null)
            {
                root.SetAsFailed("Trainer not found, please add a 'Train Model' operator into the pipeline");
                return;
            }

            if (context.TestData is null)
            {
                root.SetAsFailed("Test data not found, please add a 'Split Data' operator into the pipeline");
                return;
            }

            IDataView predictions = context.TrainedModel.Transform(context.TestData);
            //TODO: Configure score column
            var metrics = context.MLContext.Regression.Evaluate(predictions, labelColumnName: context.LabelColumn, scoreColumnName: "Score");
            var modelMetrics = new RegressionMetricsDto
            {
                LossFunction = metrics.LossFunction,
                MeanAbsoluteError = metrics.MeanAbsoluteError,
                MeanSquaredError = metrics.MeanSquaredError,
                RootMeanSquaredError = metrics.RootMeanSquaredError,
                RSquared = metrics.RSquared
            };
            var metricsSerialized = JsonSerializer.Serialize(modelMetrics);

            PrintRegressionMetrics(context.Trainer?.ToString() ?? "N/A", metrics);

            if (context.Workflow.MLModel is null)
            {
                root.SetAsFailed("Model not found, please add a 'Train Model' operator into the pipeline");
                return;
            }

            await _dbContext.Entry(context.Workflow.MLModel)
                .Reference(x => x.ModelMetrics)
                .LoadAsync();

            if (context.Workflow.MLModel.ModelMetrics is null)
            {
                context.Workflow.MLModel.ModelMetrics = new ModelMetrics
                {
                    MetricType = MetricTypeEnum.Regression,
                    Data = metricsSerialized
                };
            }
            else
            {
                context.Workflow.MLModel.ModelMetrics.MetricType = MetricTypeEnum.Regression;
                context.Workflow.MLModel.ModelMetrics.Data = metricsSerialized;
            }
            await _dbContext.SaveChangesAsync();

            root.Data.Parameters = new Dictionary<string, object>();
            root.Data.Parameters.Add("ModelMetricsId", context.Workflow.MLModel.ModelMetrics.Id);
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

    }
}
