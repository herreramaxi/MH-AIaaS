using AIaaS.WebAPI.Models.enums;
using Microsoft.ML;
using Microsoft.ML.Data;

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
            try
            {
                var mlContext = context.MLContext;
                var estimator = mlContext.Transforms.ReplaceMissingValues(context.InputOutputColumns, Microsoft.ML.Transforms.MissingValueReplacingEstimator.ReplacementMode.Mean);

                context.EstimatorChain = context.EstimatorChain is not null ?
                    context.EstimatorChain.Append(estimator) :
                    estimator;

                var transformer = context.EstimatorChain.Fit(context.DataView);
                var dataview = transformer.Transform(context.DataView);
                var preview = dataview.Preview(50);
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
                _logger.LogError((EventId)1, ex, ex.Message);
            }
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

        public class AdvertisingRow
        {
            [LoadColumn(0)]
            public float TV;
            [LoadColumn(1)]
            public float Radio;
            [LoadColumn(2)]
            public float Newspaper;
            [LoadColumn(3)]
            public float Sales;
        }

        public class AdvertisingRowPrediction
        {
            [ColumnName("Score")]
            public float Sales;
        }

    }
}
