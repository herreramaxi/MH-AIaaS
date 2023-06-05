using Microsoft.Data.Analysis;
using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using static AIaaS.WebAPI.Models.Operators.CleanDataOperator;
using static Microsoft.ML.DataOperationsCatalog;
using Microsoft.ML.Transforms;

namespace AIaaS.UnitTests
{
    public class WorkflowTests : IClassFixture<TestDatabaseFixture>, IDisposable
    {
        public TestDatabaseFixture Fixture { get; }private EfContext _dbContext;
        private readonly ITestOutputHelper _testOutputHelper;

        public WorkflowTests(TestDatabaseFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            _testOutputHelper = testOutputHelper;
            _dbContext = Fixture.CreateContext();
        }


        [Fact]
        public async Task RunWOrkflow()
        {
            var dataset = await _dbContext.Datasets.FindAsync(12);



        }

        [Fact]
        public async Task ReadFromTextFile()
        {
            var dataPath = "advertisingWithMissingValues.csv";
            var mlContext = new MLContext();
            ColumnInferenceResults columnInference = mlContext.Auto().InferColumns(dataPath, labelColumnName: "Sales",groupColumns: false);
            TextLoader loader = mlContext.Data.CreateTextLoader(columnInference.TextLoaderOptions);
            IDataView data = loader.Load(dataPath);
            var preview = data.Preview();


            using var reader = new FileStream(dataPath, FileMode.Open, FileAccess.Read);
            using var tr = new StreamReader(reader);
            var csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture);
            csvConfiguration.Delimiter = ",";
            csvConfiguration.HasHeaderRecord = true;
            using var csv = new CsvReader(tr, csvConfiguration);
            csv.Read();
            csv.ReadHeader();
            string[] headerRow = csv.Context.Reader.HeaderRecord;
            //var options = new TextLoader.Options { Separators = new[] { ',' }, HasHeader = true };
            //var textLoader = mlContext.Data.CreateTextLoader(options);
            //var data1 = textLoader.Load(dataPath);
            //var preview1 = data.Preview();
        }

        [Fact]
        public void Test()
        {
            MLContext mlContext = new MLContext(seed: 0);
            TextLoader.Column[] columns = new TextLoader.Column[] {
                new TextLoader.Column("Tv",DataKind.Single,0),
            new TextLoader.Column("Radio",DataKind.Single,1),
            new TextLoader.Column("Newspaper",DataKind.Single,2),
             new TextLoader.Column("Sales",DataKind.Single,3),
            };

            var propertyTypes = new List<(string, Type)> {
            new ("Tv", typeof(float)),
            new ("Radio", typeof(float)),
            new ("Newspaper", typeof(float)),
            new ("Sales", typeof(float))
            };
            //var propertyTypes = columnSettings.Select(x => (x.ColumnName, x.Type.ToDataType()));
            var rowType = ClassFactory.CreateType(propertyTypes);
            var schemaDefinition = SchemaDefinition.Create(rowType);

            using var fileStream = new FileStream("advertising.csv", FileMode.Open, FileAccess.Read, FileShare.None);
            using var tr = new StreamReader(fileStream);
            var methodInfo = this.GetType().GetMethods().FirstOrDefault(x => x.Name == "NewMethod");
            var processMethod = methodInfo.MakeGenericMethod(rowType);
            var baseTrainingDataView = processMethod.Invoke(this, new object[] { mlContext, tr, schemaDefinition }) as IDataView;


            //IDataView baseTrainingDataView = mlContext.Data.LoadFromTextFile("advertising.csv", columns, hasHeader: true, separatorChar: ',');
            TrainTestData trainTestSplit = mlContext.Data.TrainTestSplit(baseTrainingDataView, testFraction: 0.2);
            IDataView trainingData = trainTestSplit.TrainSet;
            IDataView testData = trainTestSplit.TestSet;

            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessPipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(AdvertisingRow.Sales))
                            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(AdvertisingRow.Tv)))
                            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(AdvertisingRow.Radio)))
                            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(AdvertisingRow.Newspaper)))
                            .Append(mlContext.Transforms.Concatenate("Features", nameof(AdvertisingRow.Tv), nameof(AdvertisingRow.Radio), nameof(AdvertisingRow.Newspaper)));


            // STEP 3: Set the training algorithm, then create and config the modelBuilder - Selected Trainer (SDCA Regression algorithm)                            
            var trainer = mlContext.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features");
            var trainingPipeline = dataProcessPipeline.Append(trainer);

            // STEP 4: Train the model fitting to the DataSet
            //The pipeline is trained on the dataset that has been loaded and transformed.
            _testOutputHelper.WriteLine("=============== Training the model ===============");
            var trainedModel = trainingPipeline.Fit(trainingData);

            // STEP 5: Evaluate the model and show accuracy stats
            _testOutputHelper.WriteLine("===== Evaluating Model's accuracy with Test data =====");

            IDataView predictions = trainedModel.Transform(testData);
            var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");

            this.PrintRegressionMetrics(trainer.ToString(), metrics);

            // STEP 6: Save/persist the trained model to a .ZIP file
            mlContext.Model.Save(trainedModel, trainingData.Schema, "model.zip");


            var taxiTripSample = new AdvertisingRow()
            {
                Tv = 232,
                Radio = 8,
                Newspaper = 8,
                Sales = 0 // To predict. Actual/Observed = 15.5
            };

            ///
            ITransformer model = mlContext.Model.Load("model.zip", out var modelInputSchema);

            // Create prediction engine related to the loaded trained model

            var predEngine = mlContext.Model.CreatePredictionEngine<AdvertisingRow, AdvertisingRowPrediction>(model);
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
            dynamicPredictionEngine = predictionMethod.Invoke(mlContext.Model, new object[] { model, true, null, null });

            //Score
            var resultprediction = predEngine.Predict(taxiTripSample);
            ///

            var predictMethod = dynamicPredictionEngine.GetType().GetMethod("Predict", new[] { runtimeType });
            var predict = predictMethod.Invoke(dynamicPredictionEngine, new[] { sample });

            _testOutputHelper.WriteLine($"**********************************************************************");
            _testOutputHelper.WriteLine($"Predicted fare: {predict.Sales:0.####}, actual fare: 18.4");
            _testOutputHelper.WriteLine($"**********************************************************************");
        }

        [Fact]
        public void Test2()
        {
            var mlContext = new MLContext();

            var propertyTypes = new List<(string, Type)> {
            new ("TV", typeof(float)),
            new ("Radio", typeof(float)),
            new ("Newspaper", typeof(float)),
            new ("Sales", typeof(float))
            };
            //var propertyTypes = columnSettings.Select(x => (x.ColumnName, x.Type.ToDataType()));
            var rowType = ClassFactory.CreateType(propertyTypes);
            var schemaDefinition = SchemaDefinition.Create(rowType);

            using var fileStream = new FileStream("advertising.csv", FileMode.Open, FileAccess.Read, FileShare.None);
            using var tr = new StreamReader(fileStream);
            var methodInfo = this.GetType().GetMethods().FirstOrDefault(x => x.Name == "NewMethod");
            var processMethod = methodInfo.MakeGenericMethod(rowType);
            var baseTrainingDataView = processMethod.Invoke(this, new object[] { mlContext, tr, schemaDefinition }) as IDataView;
            //var csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture);
            //var csv = new CsvReader(tr, csvConfiguration);
            //var records = csv.GetRecords(rowType).ToList();

            //using (FileStream stream = new FileStream("data.idv", FileMode.Create))
            //    mlContext.Data.SaveAsBinary(baseTrainingDataView, stream);

            var stream = new MemoryStream();
            mlContext.Data.SaveAsBinary(baseTrainingDataView, stream);

            stream.Seek(0, SeekOrigin.Begin);
            var mss = new MultiStreamSourceFile(stream);

            //baseTrainingDataView = mlContext.Data.LoadFromBinary("data.idv");
            baseTrainingDataView = mlContext.Data.LoadFromBinary(mss);

            //IDataView baseTrainingDataView = mlContext.Data.LoadFromTextFile("advertising.csv", columns, hasHeader: true, separatorChar: ',');
            TrainTestData trainTestSplit = mlContext.Data.TrainTestSplit(baseTrainingDataView, testFraction: 0.2);
            IDataView trainingData = trainTestSplit.TrainSet;
            IDataView testData = trainTestSplit.TestSet;

            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessPipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(AdvertisingRow.Sales))
                            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(AdvertisingRow.Tv)))
                            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(AdvertisingRow.Radio)))
                            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(AdvertisingRow.Newspaper)))
                            .Append(mlContext.Transforms.Concatenate("Features", nameof(AdvertisingRow.Tv), nameof(AdvertisingRow.Radio), nameof(AdvertisingRow.Newspaper)));


            // STEP 3: Set the training algorithm, then create and config the modelBuilder - Selected Trainer (SDCA Regression algorithm)                            
            var trainer = mlContext.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features");
            var trainingPipeline = dataProcessPipeline.Append(trainer);

            // STEP 4: Train the model fitting to the DataSet
            //The pipeline is trained on the dataset that has been loaded and transformed.
            _testOutputHelper.WriteLine("=============== Training the model ===============");
            var trainedModel = trainingPipeline.Fit(trainingData);

            // STEP 5: Evaluate the model and show accuracy stats
            _testOutputHelper.WriteLine("===== Evaluating Model's accuracy with Test data =====");

            IDataView predictions = trainedModel.Transform(testData);
            var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");

            this.PrintRegressionMetrics(trainer.ToString(), metrics);

            // STEP 6: Save/persist the trained model to a .ZIP file
            mlContext.Model.Save(trainedModel, trainingData.Schema, "model.zip");


            var taxiTripSample = new AdvertisingRow()
            {
                Tv = 232,
                Radio = 8,
                Newspaper = 8,
                Sales = 0 // To predict. Actual/Observed = 15.5
            };

            ///
            ITransformer model = mlContext.Model.Load("model.zip", out var modelInputSchema);

            // Create prediction engine related to the loaded trained model

            var predEngine = mlContext.Model.CreatePredictionEngine<AdvertisingRow, AdvertisingRowPrediction>(model);
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
            dynamicPredictionEngine = predictionMethod.Invoke(mlContext.Model, new object[] { model, true, null, null });

            //Score
            var resultprediction = predEngine.Predict(taxiTripSample);
            ///


            //var predictMethod = dynamicPredictionEngine.GetType().GetMethod("Predict", new[] { typeof(AdvertisingRow) });
            //var predict = predictMethod.Invoke(dynamicPredictionEngine, new[] { taxiTripSample });
            var predictMethod = dynamicPredictionEngine.GetType().GetMethod("Predict", new[] { runtimeType });
            var predict = predictMethod.Invoke(dynamicPredictionEngine, new[] { sample });

            _testOutputHelper.WriteLine($"**********************************************************************");
            _testOutputHelper.WriteLine($"Predicted fare: {predict.Sales:0.####}, actual fare: 18.4");
            _testOutputHelper.WriteLine($"**********************************************************************");
        }

        [Fact]
        public void Test3()
        {
            var mlContext = new MLContext();
            ColumnInferenceResults columnInference = mlContext.Auto().InferColumns("advertising.csv", labelColumnName: "Sales", groupColumns: false);
        }

        [Fact]
        public void Test3_1()
        {
            var mlContext = new MLContext();
            ColumnInferenceResults columnInference = mlContext.Auto().InferColumns("advertisingWithMissingValues.csv", labelColumnName: "Sales", groupColumns: false);
        }

        [Fact]
        public void Test4()
        {
            var ctx = new MLContext();
            ColumnInferenceResults columnInference = ctx.Auto().InferColumns("taxi-fare-full.csv", labelColumnName: "fare_amount", groupColumns: false);

            //// Create text loader
            //TextLoader loader = ctx.Data.CreateTextLoader(columnInference.TextLoaderOptions);

            //// Load data into IDataView
            //IDataView data = loader.Load(dataPath);
        }


        [Fact]
        public void Test5()
        {
            
            var dataFrame = DataFrame.LoadCsv("advertising.csv");
            var dataFrame2 = dataFrame.Description();
            var dataFrame3 = dataFrame.Info();
        }

        [Fact]
        public async  void Test6() {
            var dataset = await _dbContext.Datasets.FindAsync(1030);
            if (dataset is null)
                throw new Exception("Dataset not found");

            await _dbContext.Entry(dataset).Collection(d => d.ColumnSettings).LoadAsync();
            await _dbContext.Entry(dataset).Reference(x => x.DataViewFile).LoadAsync();

            var file = dataset.DataViewFile;

            //TODO: check how to manage usings cos if I dispose IDataview cannot be processed
            var memStream = new MemoryStream(file.Data);
            var mss = new MultiStreamSourceFile(memStream);
            var context = new WorkflowContext();
            var mlContext = new MLContext();
            context.DataView = mlContext.Data.LoadFromBinary(mss);
            context.InputOutputColumns = dataset.ColumnSettings.Select(x => new InputOutputColumnPair(x.ColumnName, x.ColumnName)).ToArray();

            var dataview = context.DataView;
            var preview0 = dataview.Preview();

            var mlContext1 = new MLContext();

            var pipeline = mlContext1.Transforms.ReplaceMissingValues(context.InputOutputColumns, Microsoft.ML.Transforms.MissingValueReplacingEstimator.ReplacementMode.Mean);
            var transformer = pipeline.Fit(dataview);
           var dataview1  =  transformer.Transform(dataview);
            var preview1 = dataview1.Preview();

            var mlContext2 = new MLContext();
            var pipeline2  = mlContext2.Transforms.NormalizeMeanVariance(context.InputOutputColumns);
            var transformer2 = pipeline2.Fit(dataview1);
            var dataview2 = transformer.Transform(dataview1);
            var preview2 = dataview2.Preview();
        }

        [Fact]
        public void Test7() {
            TextLoader.Column[] columns = new TextLoader.Column[] {
                new TextLoader.Column("TV",DataKind.Single,0),
            new TextLoader.Column("Radio",DataKind.Single,1),
            new TextLoader.Column("Newspaper",DataKind.Single,2),
             new TextLoader.Column("Sales",DataKind.Single,3),
            };
            var inputOutputColumns = columns.Select(x => new InputOutputColumnPair(x.Name, x.Name)).ToArray();
            var mlContext = new MLContext();
            var options = new TextLoader.Options()
            {
                Separators = new[] { ',' },
                HasHeader = true,
                Columns = columns,
                MissingRealsAsNaNs = true,
            };
            IDataView dataView0 = mlContext.Data.LoadFromTextFile("advertisingWithMissingValues.csv", options: options);
            var preview0 = dataView0.Preview();

            var inputOutputColumns2 = columns.Select(x => new InputOutputColumnPair(x.Name +"_Missing", x.Name)).ToArray();

            //var pipeline = mlContext.Transforms.IndicateMissingValues(inputOutputColumns2);

            var mlContext1 = new MLContext();
            var pipeline = mlContext1.Transforms.ReplaceMissingValues(inputOutputColumns, Microsoft.ML.Transforms.MissingValueReplacingEstimator.ReplacementMode.Mean);
            var transformer = pipeline.Fit(dataView0);
            var dataview1 = transformer.Transform(dataView0);
            var preview1 = dataview1.Preview();


            var mlContext2 = new MLContext();

            var pipeline2 = mlContext1.Transforms.NormalizeMeanVariance(inputOutputColumns);
            ////var pipeline = mlContext1.Transforms.NormalizeMeanVariance(outputColumnName: nameof(AdvertisingRow.TV))
            ////                .Append(mlContext1.Transforms.NormalizeMeanVariance(outputColumnName: nameof(AdvertisingRow.Radio)))
            ////                .Append(mlContext1.Transforms.NormalizeMeanVariance(outputColumnName: nameof(AdvertisingRow.Newspaper)))
            ////                 .Append(mlContext1.Transforms.NormalizeMeanVariance(outputColumnName: nameof(AdvertisingRow.Sales)));

            var dataview2 = pipeline.Fit(dataview1).Transform(dataview1);
            var preview2 = dataview2.Preview();
        }
        public IDataView NewMethod<T>(MLContext mlContext, StreamReader tr, SchemaDefinition schemaDefinition) where T : class
        {
            var csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture);
            var csv = new CsvReader(tr, csvConfiguration);
            //T record = (T)Activator.CreateInstance(typeof(T));
            //var records = csv.EnumerateRecords(record);
            var records = csv.GetRecords<T>().ToList();

            //var data = mlContext.Data.LoadFromEnumerable(records, schemaDefinition);
            var data = mlContext.Data.LoadFromEnumerable(records);
            //var preview = data.Preview();
            return data;
        }

        public IDataView NewMethod2<T>(MLContext mlContext, StreamReader tr, SchemaDefinition schemaDefinition) where T : class
        {
            var csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture);
            var csv = new CsvReader(tr, csvConfiguration);
            //T record = (T)Activator.CreateInstance(typeof(T));
            //var records = csv.EnumerateRecords(record);
            var records = csv.GetRecords<T>();

            var data = mlContext.Data.LoadFromEnumerable(records, schemaDefinition);
            //var preview = data.Preview();
            return data;
        }

        public void PrintRegressionMetrics(string name, RegressionMetrics metrics)
        {

            _testOutputHelper.WriteLine($"*************************************************");
            _testOutputHelper.WriteLine($"*       Metrics for {name} regression model      ");
            _testOutputHelper.WriteLine($"*------------------------------------------------");
            _testOutputHelper.WriteLine($"*       LossFn:        {metrics.LossFunction:0.##}");
            _testOutputHelper.WriteLine($"*       R2 Score:      {metrics.RSquared:0.##}");
            _testOutputHelper.WriteLine($"*       Absolute loss: {metrics.MeanAbsoluteError:#.##}");
            _testOutputHelper.WriteLine($"*       Squared loss:  {metrics.MeanSquaredError:#.##}");
            _testOutputHelper.WriteLine($"*       RMS loss:      {metrics.RootMeanSquaredError:#.##}");
            _testOutputHelper.WriteLine($"*************************************************");
        }
        public void Dispose()
        {
            _dbContext?.Dispose();
        }

        public class MultiStreamSourceFile : IMultiStreamSource
        {
            private MemoryStream _stream;

            public MultiStreamSourceFile(MemoryStream stream)
            {
                _stream = stream;
            }

            public int Count => 1;

            public string GetPathOrNull(int index)
            {
                return string.Empty;
            }

            public Stream Open(int index)
            {
                return _stream;
            }

            public TextReader OpenTextReader(int index)
            {
                throw new NotImplementedException();
            }
        }

        public class AdvertisingRow
        {
            [LoadColumn(0)]
            public float Tv;
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
