using Common;
using Microsoft.ML;
using Microsoft.ML.Data;
using Regression_TaxiFarePrediction;
using static Microsoft.ML.DataOperationsCatalog;

namespace ConsoleAppTest
{
    public class CreditCardFraudDetection
    {

        private static string Dataset = "creditcard10k.csv";
        private static string ModelPath = $"creditcardModel.zip";
        public void Run()
        {  //Create ML Context with seed for repeatable/deterministic results
            MLContext mlContext = new MLContext(seed: 1);

            // Create, Train, Evaluate and Save a model
            BuildTrainEvaluateAndSaveModel(mlContext);

            Console.WriteLine("Press any key to exit..");
            Console.ReadLine();
        }

        private static ITransformer? BuildTrainEvaluateAndSaveModel(MLContext mlContext)
        {
            IDataView baseTrainingDataView = mlContext.Data.LoadFromTextFile<TransactionObservation>(Dataset, separatorChar: ',', hasHeader: true,allowQuoting: true);

            // Split the data 80:20 into train and test sets, train and evaluate.
            TrainTestData trainTestData = mlContext.Data.TrainTestSplit(baseTrainingDataView, testFraction: 0.2, seed: 1);
            IDataView trainData = trainTestData.TrainSet;
            IDataView testData = trainTestData.TestSet;

            //Inspect TestDataView to make sure there are true and false observations in test dataset, after spliting 
            InspectData(mlContext, testData, 4);

            // Train Model
            (ITransformer model, string trainerName) = TrainModel(mlContext, trainData);

            // Evaluate quality of Model
            EvaluateModel(mlContext, model, testData, trainerName);

            // Save model
            SaveModel(mlContext, model, ModelPath, testData.Schema);

            Console.WriteLine("=============== Press any key ===============");
            Console.ReadKey();

            return null;
        }

        public static void InspectData(MLContext mlContext, IDataView data, int records)
        {
            //We want to make sure we have True and False observations

            Console.WriteLine("Show 4 fraud transactions (true)");
            ShowObservationsFilteredByLabel(mlContext, data, label: true, count: records);

            Console.WriteLine("Show 4 NOT-fraud transactions (false)");
            ShowObservationsFilteredByLabel(mlContext, data, label: false, count: records);
        }

        public static void ShowObservationsFilteredByLabel(MLContext mlContext, IDataView dataView, bool label = true, int count = 2)
        {
            // Convert to an enumerable of user-defined type. 
            var data = mlContext.Data.CreateEnumerable<TransactionObservation>(dataView, reuseRowObject: false)
                                            .Where(x => x.Label == label)
                                            // Take a couple values as an array.
                                            .Take(count)
                                            .ToList();

            // print to console
            data.ForEach(row => { row.PrintToConsole(); });
        }

        public static (ITransformer model, string trainerName) TrainModel(MLContext mlContext, IDataView trainDataView)
        {
            //Get all the feature column names (All except the Label and the IdPreservationColumn)
            string[] featureColumnNames = trainDataView.Schema.AsQueryable()
                .Select(column => column.Name)                               // Get alll the column names
                .Where(name => name != nameof(TransactionObservation.Label)) // Do not include the Label column
                .Where(name => name != "IdPreservationColumn")               // Do not include the IdPreservationColumn/StratificationColumn
                .Where(name => name != "Time")                               // Do not include the Time column. Not needed as feature column
                .ToArray();

            // Create the data process pipeline
            IEstimator<ITransformer> dataProcessPipeline = mlContext.Transforms.Concatenate("Features", featureColumnNames)
                                            .Append(mlContext.Transforms.DropColumns(new string[] { "Time" }))
                                            .Append(mlContext.Transforms.NormalizeMeanVariance(inputColumnName: "Features",
                                                                                 outputColumnName: "FeaturesNormalizedByMeanVar"));

            // (OPTIONAL) Peek data (such as 2 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
            ConsoleHelper.PeekDataViewInConsole(mlContext, trainDataView, dataProcessPipeline, 2);
            ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", trainDataView, dataProcessPipeline, 1);

            // Set the training algorithm
            var trainer = mlContext.BinaryClassification.Trainers.FastTree(labelColumnName: nameof(TransactionObservation.Label),
                                                                                                featureColumnName: "FeaturesNormalizedByMeanVar",
                                                                                                numberOfLeaves: 20,
                                                                                                numberOfTrees: 100,
                                                                                                minimumExampleCountPerLeaf: 10,
                                                                                                learningRate: 0.2);

            var trainingPipeline = dataProcessPipeline.Append(trainer);

            ConsoleHelper.ConsoleWriteHeader("=============== Training model ===============");

            var model = trainingPipeline.Fit(trainDataView);

            ConsoleHelper.ConsoleWriteHeader("=============== End of training process ===============");

            // Append feature contribution calculator in the pipeline. This will be used
            // at prediction time for explainability. 
            var fccPipeline = model.Append(mlContext.Transforms
                .CalculateFeatureContribution(model.LastTransformer)
                .Fit(dataProcessPipeline.Fit(trainDataView).Transform(trainDataView)));

            return (fccPipeline, fccPipeline.ToString());

        }

        private static void EvaluateModel(MLContext mlContext, ITransformer model, IDataView testDataView, string trainerName)
        {
            // Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            var predictions = model.Transform(testDataView);

            var metrics = mlContext.BinaryClassification.Evaluate(data: predictions,
                                                                  labelColumnName: nameof(TransactionObservation.Label),
                                                                  scoreColumnName: "Score");

            ConsoleHelper.PrintBinaryClassificationMetrics(trainerName, metrics);
        }
        private static void SaveModel(MLContext mlContext, ITransformer model, string modelFilePath, DataViewSchema trainingDataSchema)
        {
            mlContext.Model.Save(model, trainingDataSchema, modelFilePath);

            Console.WriteLine("Saved model to " + modelFilePath);
        }
        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }

    public interface IModelEntity
    {
        void PrintToConsole();
    }

    public class TransactionObservation : IModelEntity
    {
        // Note we're not loading the 'Time' column, since que don't need it as a feature
        [LoadColumn(0)]
        public float Time;

        [LoadColumn(1)]
        public float V1;

        [LoadColumn(2)]
        public float V2;

        [LoadColumn(3)]
        public float V3;

        [LoadColumn(4)]
        public float V4;

        [LoadColumn(5)]
        public float V5;

        [LoadColumn(6)]
        public float V6;

        [LoadColumn(7)]
        public float V7;

        [LoadColumn(8)]
        public float V8;

        [LoadColumn(9)]
        public float V9;

        [LoadColumn(10)]
        public float V10;

        [LoadColumn(11)]
        public float V11;

        [LoadColumn(12)]
        public float V12;

        [LoadColumn(13)]
        public float V13;

        [LoadColumn(14)]
        public float V14;

        [LoadColumn(15)]
        public float V15;

        [LoadColumn(16)]
        public float V16;

        [LoadColumn(17)]
        public float V17;

        [LoadColumn(18)]
        public float V18;

        [LoadColumn(19)]
        public float V19;

        [LoadColumn(20)]
        public float V20;

        [LoadColumn(21)]
        public float V21;

        [LoadColumn(22)]
        public float V22;

        [LoadColumn(23)]
        public float V23;

        [LoadColumn(24)]
        public float V24;

        [LoadColumn(25)]
        public float V25;

        [LoadColumn(26)]
        public float V26;

        [LoadColumn(27)]
        public float V27;

        [LoadColumn(28)]
        public float V28;

        [LoadColumn(29)]
        public float Amount;

        [LoadColumn(30)]
        public bool Label;

        public void PrintToConsole()
        {
            Console.WriteLine($"Label: {Label}");
            Console.WriteLine($"Features: [V1] {V1} [V2] {V2} [V3] {V3} ... [V28] {V28} Amount: {Amount}");
        }

        //public static List<KeyValuePair<ColumnRole, string>>  Roles() {
        //    return new List<KeyValuePair<ColumnRole, string>>() {
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Label, "Label"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V1"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V2"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V3"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V4"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V5"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V6"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V7"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V8"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V9"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V10"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V11"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V12"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V13"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V14"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V15"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V16"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V17"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V18"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V19"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V20"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V21"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V22"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V23"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V24"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V25"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V26"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V27"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V28"),
        //            new KeyValuePair<ColumnRole, string>(new ColumnRole("Amount"), ""),

        //        };
        //}
    }

}




