using AIaaS.WebAPI.Interfaces;
using AIaaS.WebAPI.Models.enums;
using Microsoft.ML;
using Microsoft.ML.Data;
using static Microsoft.ML.DataOperationsCatalog;

namespace AIaaS.WebAPI.Models.Operators
{
    [Operator("CleanData", OperatorType.Clean, 2)]
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
                TrainTestData trainTestSplit = mlContext.Data.TrainTestSplit(context.DataView, testFraction: 0.2);
                IDataView trainingData = trainTestSplit.TrainSet;
                IDataView testData = trainTestSplit.TestSet;
                // STEP 2: Common data process configuration with pipeline data transformations
                var dataProcessPipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "Sales")
                                .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: "TV"))
                                .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: "Radio"))
                                .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: "Newspaper"))
                                .Append(mlContext.Transforms.Concatenate("Features", "TV", "Radio", "Newspaper"));

                //mlContext.Transforms.ReplaceMissingValues("asda", replacementMode: Microsoft.ML.Transforms.MissingValueReplacingEstimator.ReplacementMode.Minimum);
                //mlContext.Transforms.Conversion.ConvertType("inputColName", outputKind: DataKind.Single);

                var trainer = mlContext.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features");
                var trainingPipeline = dataProcessPipeline.Append(trainer);
                 var trainedModel1 = trainingPipeline.Fit(trainingData);
                IDataView predictions = trainedModel1.Transform(testData);
                var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");

                PrintRegressionMetrics(trainer.ToString(), metrics);
                mlContext.Model.Save(trainedModel1, trainingData.Schema, "model.zip");

                ITransformer trainedModel = mlContext.Model.Load("model.zip", out var modelInputSchema);



                var predEngine = mlContext.Model.CreatePredictionEngine<AdvertisingRow, AdvertisingRowPrediction>(trainedModel);
                var runtimeType = ClassFactory.CreateType(modelInputSchema);
                var predictionObject = ClassFactory.CreateObject(new string[] { "Sales" }, new Type[] { typeof(float) }, new[] { true });

                dynamic sample = Activator.CreateInstance(runtimeType);//ClassFactory.CreateObject(new string[] { "Tv", "Radio", "Newspaper", "Sales" }, new Type[] { typeof(float), typeof(float), typeof(float), typeof(float) });
                sample.TV = 232;
                sample.Radio = 8;
                sample.Newspaper = 8;
                sample.Sales = 0;

                //mlContext.Model.CreatePredictionEngine<int>()
                dynamic dynamicPredictionEngine;
                //SchemaDefinition inputSchemaDefinition = null, SchemaDefinition outputSchemaDefinition = null
                var genericPredictionMethod = mlContext.Model.GetType().GetMethod("CreatePredictionEngine", new[] { typeof(ITransformer), typeof(bool), typeof(SchemaDefinition), typeof(SchemaDefinition) });
                //var predictionMethod = genericPredictionMethod.MakeGenericMethod(typeof(AdvertisingRow),typeof(AdvertisingRowPrediction));// predictionObject.GetType());
                var predictionMethod = genericPredictionMethod.MakeGenericMethod(runtimeType, predictionObject.GetType());// predictionObject.GetType());
                dynamicPredictionEngine = predictionMethod.Invoke(mlContext.Model, new object[] { trainedModel, true, null, null });

                var taxiTripSample = new AdvertisingRow()
                {
                    TV = 232,
                    Radio = 8,
                    Newspaper = 8,
                    Sales = 0 // To predict. Actual/Observed = 15.5
                };
                //Score
                var resultprediction = predEngine.Predict(taxiTripSample);

                var predictMethod = dynamicPredictionEngine.GetType().GetMethod("Predict", new[] { runtimeType });
                var predict = predictMethod.Invoke(dynamicPredictionEngine, new[] { sample });
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
