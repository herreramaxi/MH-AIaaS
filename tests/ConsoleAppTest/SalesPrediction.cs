using Common;
using Microsoft.ML;
using Microsoft.ML.Data;
using PLplot;
using Regression_TaxiFarePrediction;
using System.Diagnostics;
using System.Globalization;
using static ConsoleAppTest.SalesPrediction;
using static Microsoft.ML.DataOperationsCatalog;

namespace ConsoleAppTest
{
    public class SalesPrediction
    {

        private static string Dataset = "advertising.csv";
        private static string ModelPath = $"advertising.zip";
        public void Run()
        {  //Create ML Context with seed for repeatable/deterministic results
            MLContext mlContext = new MLContext(seed: 0);

            // Create, Train, Evaluate and Save a model
            BuildTrainEvaluateAndSaveModel(mlContext);

            // Make a single test prediction loding the model from .ZIP file
            TestSinglePrediction(mlContext);

            // Paint regression distribution chart for a number of elements read from a Test DataSet file
            PlotRegressionChart(mlContext, Dataset, 50, new string[] { });

            Console.WriteLine("Press any key to exit..");
            Console.ReadLine();
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

        private static ITransformer BuildTrainEvaluateAndSaveModel(MLContext mlContext)
        {
            TextLoader.Column[] columns = new TextLoader.Column[] {
                new TextLoader.Column("Tv",DataKind.Single,0),
            new TextLoader.Column("Radio",DataKind.Single,1),
            new TextLoader.Column("Newspaper",DataKind.Single,2),
             new TextLoader.Column("Sales",DataKind.Single,3),
            };
       
            // STEP 1: Common data loading configuration
            //IDataView baseTrainingDataView = mlContext.Data.LoadFromTextFile<AdvertisingRow>(Dataset, hasHeader: true, separatorChar: ',');
            IDataView baseTrainingDataView = mlContext.Data.LoadFromTextFile(Dataset, columns, hasHeader: true, separatorChar: ',');            
            TrainTestData trainTestSplit = mlContext.Data.TrainTestSplit(baseTrainingDataView, testFraction: 0.2);
            IDataView trainingData = trainTestSplit.TrainSet;
            IDataView testData = trainTestSplit.TestSet;
                                    
            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessPipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(AdvertisingRow.Sales))
                            .Append(  mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(AdvertisingRow.Tv)))
                            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(AdvertisingRow.Radio)))
                            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(AdvertisingRow.Newspaper)))
                            .Append(mlContext.Transforms.Concatenate("Features", nameof(AdvertisingRow.Tv)));

            
            // (OPTIONAL) Peek data (such as 5 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
            ConsoleHelper.PeekDataViewInConsole(mlContext, trainingData, dataProcessPipeline, 5);
            ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", trainingData, dataProcessPipeline, 5);

            
            // STEP 3: Set the training algorithm, then create and config the modelBuilder - Selected Trainer (SDCA Regression algorithm)                            
            var trainer = mlContext.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features");
            var trainingPipeline = dataProcessPipeline.Append(trainer);

            // STEP 4: Train the model fitting to the DataSet
            //The pipeline is trained on the dataset that has been loaded and transformed.
            Console.WriteLine("=============== Training the model ===============");
            var trainedModel = trainingPipeline.Fit(trainingData);

            // STEP 5: Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");

            IDataView predictions = trainedModel.Transform(testData);
            var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");

            Common.ConsoleHelper.PrintRegressionMetrics(trainer.ToString(), metrics);

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

            var taxiTripSample = new AdvertisingRow()
            {
                Tv = 232,
                Radio = 8,
                Newspaper = 8,
                Sales = 0 // To predict. Actual/Observed = 15.5
            };

            ///
            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model

            var predEngine = mlContext.Model.CreatePredictionEngine<AdvertisingRow, AdvertisingRowPrediction>(trainedModel);
            var runtimeType = ClassFactory.CreateType(modelInputSchema);
            var predictionObject = ClassFactory.CreateObject(new string[] { "Sales" }, new Type[] { typeof(float) }, new[] { true });

            dynamic sample = Activator.CreateInstance(runtimeType);//ClassFactory.CreateObject(new string[] { "Tv", "Radio", "Newspaper", "Sales" }, new Type[] { typeof(float), typeof(float), typeof(float), typeof(float) });
            sample.Tv = 232;
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

            //Score
            var resultprediction = predEngine.Predict(taxiTripSample);
            ///


            //var predictMethod = dynamicPredictionEngine.GetType().GetMethod("Predict", new[] { typeof(AdvertisingRow) });
            //var predict = predictMethod.Invoke(dynamicPredictionEngine, new[] { taxiTripSample });
            var predictMethod = dynamicPredictionEngine.GetType().GetMethod("Predict", new[] { runtimeType });
            var predict = predictMethod.Invoke(dynamicPredictionEngine, new[] { sample });

            Console.WriteLine($"**********************************************************************");
            Console.WriteLine($"Predicted fare: {predict.Sales:0.####}, actual fare: 18.4");
            Console.WriteLine($"**********************************************************************");
        }

        private static void PlotRegressionChart(MLContext mlContext,
                                                string testDataSetPath,
                                                int numberOfRecordsToRead,
                                                string[] args)
        {
            ITransformer trainedModel;
            using var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            trainedModel = mlContext.Model.Load(stream, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model
            var predFunction = mlContext.Model.CreatePredictionEngine<AdvertisingRow, AdvertisingRowPrediction>(trainedModel);
            string chartFileName = "";
            using (var pl = new PLStream())
            {
                // use SVG backend and write to SineWaves.svg in current directory
                if (args.Length == 1 && args[0] == "svg")
                {
                    pl.sdev("svg");
                    chartFileName = "TaxiRegressionDistribution.svg";
                    pl.sfnam(chartFileName);
                }
                else
                {
                    pl.sdev("pngcairo");
                    chartFileName = "TaxiRegressionDistribution.png";
                    pl.sfnam(chartFileName);
                }

                // use white background with black foreground
                pl.spal0("cmap0_alternate.pal");

                // Initialize plplot
                pl.init();

                // set axis limits
                const int xMinLimit = 0;
                const int xMaxLimit = 220; //Rides larger than $35 are not shown in the chart
                const int yMinLimit = 0;
                const int yMaxLimit = 40;  //Rides larger than $35 are not shown in the chart
                pl.env(xMinLimit, xMaxLimit, yMinLimit, yMaxLimit, AxesScale.Independent, AxisBox.BoxTicksLabelsAxes);

                // Set scaling for mail title text 125% size of default
                pl.schr(0, 1.25);

                // The main title
                pl.lab("Measured", "Predicted", "Distribution of Sales Prediction");

                // plot using different colors
                // see http://plplot.sourceforge.net/examples.php?demo=02 for palette indices
                pl.col0(1);

                int totalNumber = numberOfRecordsToRead;
                var testData = new AdvertisingCsvReader().GetDataFromCsv(testDataSetPath, totalNumber).ToList();

                //This code is the symbol to paint
                char code = (char)9;

                // plot using other color
                //pl.col0(9); //Light Green
                //pl.col0(4); //Red
                pl.col0(2); //Blue

                double yTotal = 0;
                double xTotal = 0;
                double xyMultiTotal = 0;
                double xSquareTotal = 0;

                for (int i = 0; i < testData.Count; i++)
                {
                    var x = new double[1];
                    var y = new double[1];

                    //Make Prediction
                    var FarePrediction = predFunction.Predict(testData[i]);

                    x[0] = testData[i].Tv;
                    y[0] = FarePrediction.Sales;

                    //Paint a dot
                    pl.poin(x, y, code);

                    xTotal += x[0];
                    yTotal += y[0];

                    double multi = x[0] * y[0];
                    xyMultiTotal += multi;

                    double xSquare = x[0] * x[0];
                    xSquareTotal += xSquare;

                    double ySquare = y[0] * y[0];

                    Console.WriteLine($"-------------------------------------------------");
                    Console.WriteLine($"Predicted : {FarePrediction.Sales}");
                    Console.WriteLine($"Actual:    {testData[i].Sales}");
                    Console.WriteLine($"-------------------------------------------------");
                }

                // Regression Line calculation explanation:
                // https://www.khanacademy.org/math/statistics-probability/describing-relationships-quantitative-data/more-on-regression/v/regression-line-example

                double minY = yTotal / totalNumber;
                double minX = xTotal / totalNumber;
                double minXY = xyMultiTotal / totalNumber;
                double minXsquare = xSquareTotal / totalNumber;

                double m = ((minX * minY) - minXY) / ((minX * minX) - minXsquare);

                double b = minY - (m * minX);

                //Generic function for Y for the regression line
                // y = (m * x) + b;

                double x1 = 1;
                //Function for Y1 in the line
                double y1 = (m * x1) + b;

                double x2 = 39;
                //Function for Y2 in the line
                double y2 = (m * x2) + b;

                var xArray = new double[2];
                var yArray = new double[2];
                xArray[0] = x1;
                yArray[0] = y1;
                xArray[1] = x2;
                yArray[1] = y2;

                pl.col0(4);
                pl.line(xArray, yArray);

                // end page (writes output to disk)
                pl.eop();

                // output version of PLplot
                pl.gver(out var verText);
                Console.WriteLine("PLplot version " + verText);

            } // the pl object is disposed here

            // Open Chart File In Microsoft Photos App (Or default app, like browser for .svg)

            Console.WriteLine("Showing chart...");
            var p = new Process();
            string chartFileNamePath = @".\" + chartFileName;
            p.StartInfo = new ProcessStartInfo(chartFileNamePath)
            {
                UseShellExecute = true
            };
            p.Start();
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }

    public class AdvertisingCsvReader
    {
        public IEnumerable<AdvertisingRow> GetDataFromCsv(string dataLocation, int numMaxRecords)
        {
            IEnumerable<AdvertisingRow> records =
                File.ReadAllLines(dataLocation)
                .Skip(1)
                .Select(x => x.Split(','))
                .Select(x => new AdvertisingRow()
                {
                    Tv = float.Parse(x[0], CultureInfo.InvariantCulture),
                    Radio = float.Parse(x[1], CultureInfo.InvariantCulture),
                    Newspaper = float.Parse(x[2], CultureInfo.InvariantCulture),
                    Sales = float.Parse(x[3], CultureInfo.InvariantCulture),
                })
                .Take<AdvertisingRow>(numMaxRecords);

            return records;
        }
    }

}

