using Common;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using static Microsoft.ML.DataOperationsCatalog;

namespace ConsoleAppTest
{
    public class TitanicPrediction
    {
        private static string Dataset = "titanic.csv";
        private static string ModelPath = "titanic.zip";
        public void Run()
        {  //Create ML Context with seed for repeatable/deterministic results
            MLContext mlContext = new MLContext(seed: 0);

            // Create, Train, Evaluate and Save a model
            BuildTrainEvaluateAndSaveModel(mlContext);

            // Make a single test prediction loding the model from .ZIP file
            TestSinglePrediction(mlContext);

            // Paint regression distribution chart for a number of elements read from a Test DataSet file
            //PlotRegressionChart(mlContext, Dataset, 50, new string[] { });

            Console.WriteLine("Press any key to exit..");
            Console.ReadLine();
        }

        public class TitanicRow
        {
            [LoadColumn(0)]
            public int PassengerId;
            [LoadColumn(1)]
            public bool Survived;
            [LoadColumn(2)]
            public float Pclass;
            [LoadColumn(3)]
            public string Name;
            [LoadColumn(4)]
            public string Sex;
            [LoadColumn(5)]
            public float Age;
            [LoadColumn(6)]
            public float SibSp;
            [LoadColumn(7)]
            public float Parch;
            [LoadColumn(8)]
            public string Ticket;
            [LoadColumn(9)]
            public float Fare;
            [LoadColumn(10)]
            public string Cabin;
            [LoadColumn(11)]
            public string Embarked;
        }

        public class TitanicRowPrediction
        {
            [ColumnName("PredictedLabel")]
            public bool Survived2;

            [ColumnName("Score")]
            public float Score2;
        }

        private static ITransformer BuildTrainEvaluateAndSaveModel(MLContext mlContext)
        {
            //PassengerId	Survived	Pclass	Name	Sex	Age	SibSp	Parch	Ticket	Fare	Cabin	Embarked
            var columnsToBeDropped = new string[] { "PassengerId", "Name", "Ticket", "Cabin" };

            // STEP 1: Common data loading configuration
            var options = new TextLoader.Options
            {
                AllowQuoting = true,
                HasHeader = true,
                MissingRealsAsNaNs = true,
                Separators = new[] { ',' }
            };
            IDataView baseTrainingDataView = mlContext.Data.LoadFromTextFile<TitanicRow>(Dataset, options);
            TrainTestData trainTestSplit = mlContext.Data.TrainTestSplit(baseTrainingDataView, testFraction: 0.2);
            IDataView trainingData = trainTestSplit.TrainSet;
            IDataView testData = trainTestSplit.TestSet;


            var inputOutput = (new string[] {  "Pclass", "Age", "SibSp", "Parch", "Fare" }).Select(x => new InputOutputColumnPair(x, x)).ToArray();
            var categorical = (new string[] { "Sex", "Embarked" }).Select(x => new InputOutputColumnPair(x, x)).ToArray();
            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessPipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(TitanicRow.Survived))
            .Append(mlContext.Transforms.DropColumns(columnsToBeDropped))
            .Append(mlContext.Transforms.ReplaceMissingValues(inputOutput, MissingValueReplacingEstimator.ReplacementMode.Mean))
            //.Append(mlContext.Transforms.Conversion.ConvertType("Survived", outputKind: DataKind.Boolean))
            .Append(mlContext.Transforms.Categorical.OneHotEncoding(categorical))
            //.Append(mlContext.Transforms.NormalizeMeanVariance("Fare"))
               .Append(mlContext.Transforms.Concatenate("Features", new string[] { "Pclass", "Sex", "Age", "SibSp", "Parch", "Fare", "Embarked" }));

            //var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Survived")
            //    .Append(mlContext.Transforms.Text.FeaturizeText("YourStringColumn"))
            //    .Append(mlContext.Transforms.ReplaceMissingValues("YourStringColumn", replacementMode: MissingValueReplacingEstimator.ReplacementMode.MostFrequent))
            //    .Append(mlContext.Transforms.Conversion.MapKeyToValue("Survived"));

            // (OPTIONAL) Peek data (such as 5 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
            ConsoleHelper.PeekDataViewInConsole(mlContext, trainingData, dataProcessPipeline, 10);
            ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", trainingData, dataProcessPipeline, 10);

            // STEP 3: Set the training algorithm, then create and config the modelBuilder - Selected Trainer (SDCA Regression algorithm)                            
            var trainer = mlContext.BinaryClassification.Trainers.LbfgsLogisticRegression(labelColumnName: "Label", featureColumnName: "Features");
            var trainingPipeline = dataProcessPipeline.Append(trainer);

            // STEP 4: Train the model fitting to the DataSet
            //The pipeline is trained on the dataset that has been loaded and transformed.
            Console.WriteLine("=============== Training the model ===============");
            var trainedModel = trainingPipeline.Fit(trainingData);

            // STEP 5: Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");

            IDataView predictions = trainedModel.Transform(testData);
            var metrics = mlContext.BinaryClassification.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");

            Common.ConsoleHelper.PrintBinaryClassificationMetrics(trainer.ToString(), metrics);

            // STEP 6: Save/persist the trained model to a .ZIP file
            mlContext.Model.Save(trainedModel, trainingData.Schema, ModelPath);

            Console.WriteLine("The model is saved to {0}", ModelPath);

            return trainedModel;
        }

        private static void TestSinglePrediction(MLContext mlContext)
        {
            //Sample: 
            //vendor_id,rate_code,passenger_count,trip_time_in_secs,trip_distance,payment_type,fare_amount
            //VTS,1,1,1140,3.75,CRD,15.5

            var sample = new TitanicRow()
            {
                Pclass = 3,
                Sex = "female",
                Age = 27,
                SibSp = 0,
                Parch = 2,
                Fare = 11f,
                Embarked = "S"
            };

            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model

            var predEngine = mlContext.Model.CreatePredictionEngine<TitanicRow, TitanicRowPrediction>(trainedModel);
            var predicted = predEngine.Predict(sample);

            Console.WriteLine($"**********************************************************************");
            Console.WriteLine($"Predicted fare: {predicted.Survived2:0.####}, actual fare: 18.4");
            Console.WriteLine($"**********************************************************************");
        }

    }

}

