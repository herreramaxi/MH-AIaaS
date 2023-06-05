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
        public override async Task Execute(WorkflowContext context, Dtos.WorkflowNodeDto root)
        {
            //context.TrainedModel = context.MLContext.Model.Load("model.zip", out var modelInputSchema);
            //var workflow = await _dbContext.Workflows.FindAsync(context.Workflow.Id);
            //await _dbContext.Entry(workflow).Reference(x=> x.MLModel).LoadAsync();            
            //using var memStream = new MemoryStream(workflow.MLModel.Data);
            //context.TrainedModel = context.MLContext.Model.Load(memStream, out var inputSchema);
            if (string.IsNullOrEmpty(context.LabelColumn))
            {
                root.Error("No label column found on the pipeline, please select a clabel column from a 'Train Model' operator");
                return;
            }

            IDataView predictions = context.TrainedModel.Transform(context.TestData);
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

            PrintRegressionMetrics(context.Trainer.ToString(), metrics);

            if (context.Workflow.MLModel is null)
            {
                root.Error("Model not found");
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

            root.Success();
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
