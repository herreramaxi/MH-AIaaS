﻿using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Models.enums;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace AIaaS.WebAPI.Models.Operators
{
    [Operator("Evaluate", OperatorType.Evaluate, 5)]
    [OperatorParameter("Variable Name", "The name of the new or existing variable", "text")]
    [OperatorParameter("Value", "Javascript expression for the value", "text")]
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

            IDataView predictions = context.TrainedModel.Transform(context.TestData);
            var metrics = context.MLContext.Regression.Evaluate(predictions, labelColumnName: "Sales", scoreColumnName: "Score");

            PrintRegressionMetrics(context.Trainer.ToString(), metrics);             
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