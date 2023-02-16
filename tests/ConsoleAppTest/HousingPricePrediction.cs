using Common;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using PLplot;
using Regression_TaxiFarePrediction;
using Regression_TaxiFarePrediction.DataStructures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.ML.DataOperationsCatalog;

namespace ConsoleAppTest
{
    public class HousingPricePrediction
    {
        private static string TrainDataRelativePath = $"housing.csv";
        private static string TestDataRelativePath = $"housing.csv";

        private static string TrainDataPath = GetAbsolutePath(TrainDataRelativePath);
        private static string TestDataPath = GetAbsolutePath(TestDataRelativePath);

        private static string BaseModelsRelativePath = @"../../../../MLModels";
        private static string ModelRelativePath = $"Housing.zip";

        private static string ModelPath = GetAbsolutePath(ModelRelativePath);
        public void Run()
        {
            //Create ML Context with seed for repeatable/deterministic results
            MLContext mlContext = new MLContext(seed: 0);

            // Create, Train, Evaluate and Save a model
            BuildTrainEvaluateAndSaveModel(mlContext);

            // Make a single test prediction loding the model from .ZIP file
            TestSinglePrediction(mlContext);

            // Paint regression distribution chart for a number of elements read from a Test DataSet file
            PlotRegressionChart(mlContext, TestDataPath, 50, new string[] { });

            Console.WriteLine("Press any key to exit..");
            Console.ReadLine();
        }

        public class HousingRow
        {
            [LoadColumn(0)]
            public float Longitude;

            [LoadColumn(1)]
            public float Latitude;

            [LoadColumn(2)]
            public float HousingMedianAge;

            [LoadColumn(3)]
            public float TotalRooms;

            [LoadColumn(4)]
            public float TotalBedrooms;

            [LoadColumn(5)]
            public float Population;
            [LoadColumn(6)]
            public float HouseHolds;
            [LoadColumn(7)]
            public float MedianIncome;
            [LoadColumn(8)]
            public string OceanProximity;
            [LoadColumn(9)]
            public float MedianHouseValue;


        }

        private static ITransformer BuildTrainEvaluateAndSaveModel(MLContext mlContext)
        {
            // STEP 1: Common data loading configuration


            IDataView baseTrainingDataView = mlContext.Data.LoadFromTextFile<HousingRow>(TrainDataPath, hasHeader: true, separatorChar: ',');
            TrainTestData trainTestSplit = mlContext.Data.TrainTestSplit(baseTrainingDataView, testFraction: 0.2);
            IDataView trainingData = trainTestSplit.TrainSet;
            IDataView testData = trainTestSplit.TestSet;

            //Sample code of removing extreme data like "outliers" for FareAmounts higher than $150 and lower than $1 which can be error-data 
            //var cnt = baseTrainingDataView.GetColumn<float>(nameof(TaxiTrip.FareAmount)).Count();
            //IDataView trainingDataView = mlContext.Data.FilterRowsByColumn(baseTrainingDataView, nameof(HousingRow.MedianHouseValue), lowerBound: 1, upperBound: 150);
            //var cnt2 = trainingDataView.GetColumn<float>(nameof(TaxiTrip.FareAmount)).Count();

            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessPipeline = mlContext.Transforms
           
                .CopyColumns(outputColumnName: "Label", inputColumnName: nameof(HousingRow.MedianHouseValue))                 
                           .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(HousingRow.Longitude)))
                            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(HousingRow.Latitude)))
                               .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(HousingRow.HousingMedianAge)))
                               .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(HousingRow.TotalRooms)))
                               .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(HousingRow.TotalBedrooms)))
                               .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(HousingRow.Population)))
                               .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(HousingRow.HouseHolds)))
                               .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(HousingRow.MedianIncome)))
                               .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "OceanProximityEncoded", inputColumnName: nameof(HousingRow.OceanProximity)))

                            .Append(mlContext.Transforms.Concatenate("Features",
                            nameof(HousingRow.Longitude),
                            nameof(HousingRow.Latitude),
                            nameof(HousingRow.HousingMedianAge),
                            nameof(HousingRow.TotalRooms),
                            nameof(HousingRow.TotalBedrooms),
                            nameof(HousingRow.Population),
                            nameof(HousingRow.HouseHolds),
                            nameof(HousingRow.MedianIncome),
                            "OceanProximityEncoded"
                            ));

            // (OPTIONAL) Peek data (such as 5 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
            ConsoleHelper.PeekDataViewInConsole(mlContext, baseTrainingDataView, dataProcessPipeline, 5);
            ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", baseTrainingDataView, dataProcessPipeline, 5);

            // STEP 3: Set the training algorithm, then create and config the modelBuilder - Selected Trainer (SDCA Regression algorithm)                            
            var trainer = mlContext.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features");
            var trainingPipeline = dataProcessPipeline.Append(trainer);

            // STEP 4: Train the model fitting to the DataSet
            //The pipeline is trained on the dataset that has been loaded and transformed.
            Console.WriteLine("=============== Training the model ===============");

            //TrainTestData dataSplit = mlContext.Data.TrainTestSplit(baseTrainingDataView, testFraction: 0.2);

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

            var taxiTripSample = new HousingRow()
            {
                Latitude = (float)-121.23,
                Longitude = (float)37.88,
                HousingMedianAge = 41,
                TotalRooms = 880,
                TotalBedrooms = 129,
                Population = 322,
                HouseHolds = 126,
                MedianIncome = (float)8.3252,
                OceanProximity = "INLAND",
                MedianHouseValue = 0 // To predict. Actual/Observed = 15.5
            };

            ///
            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model
            var predEngine = mlContext.Model.CreatePredictionEngine<HousingRow, HousingRowPrediction>(trainedModel);

            //Score
            var resultprediction = predEngine.Predict(taxiTripSample);
            ///

            Console.WriteLine($"**********************************************************************");
            Console.WriteLine($"Predicted fare: {resultprediction.MedianHouseValue:0.####}, actual fare: 15.5");
            Console.WriteLine($"**********************************************************************");
        }

        private static void PlotRegressionChart(MLContext mlContext,
                                                string testDataSetPath,
                                                int numberOfRecordsToRead,
                                                string[] args)
        {
            ITransformer trainedModel;
            using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                trainedModel = mlContext.Model.Load(stream, out var modelInputSchema);
            }

            // Create prediction engine related to the loaded trained model
            var predFunction = mlContext.Model.CreatePredictionEngine<HousingRow, HousingRowPrediction>(trainedModel);

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
                const int xMinLimit = -200;
                const int xMaxLimit = 900000; //Rides larger than $35 are not shown in the chart
                const int yMinLimit = 0;
                const int yMaxLimit = 900000;  //Rides larger than $35 are not shown in the chart
                pl.env(xMinLimit, xMaxLimit, yMinLimit, yMaxLimit, AxesScale.Independent, AxisBox.BoxTicksLabelsAxes);
             
                // Set scaling for mail title text 125% size of default
                pl.schr(0, 1.25);

                // The main title
                pl.lab("Measured", "Predicted", "Distribution of median_house_value Fare Prediction");

                // plot using different colors
                // see http://plplot.sourceforge.net/examples.php?demo=02 for palette indices
                pl.col0(1);

                int totalNumber = numberOfRecordsToRead;
                var testData = new HousingRowCsvReader().GetDataFromCsv(testDataSetPath, totalNumber).ToList();

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

                    x[0] = testData[i].MedianHouseValue;
                    y[0] = FarePrediction.MedianHouseValue;

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
                    Console.WriteLine($"Predicted : {FarePrediction.MedianHouseValue}");
                    Console.WriteLine($"Actual:    {testData[i].MedianHouseValue}");
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

        public class HousingRowCsvReader
        {
            public IEnumerable<HousingRow> GetDataFromCsv(string dataLocation, int numMaxRecords)
            {
                IEnumerable<HousingRow> records =
                    File.ReadAllLines(dataLocation)
                    .Skip(1)
                    .Select(x => x.Split(','))
                    .Select(x => new HousingRow()
                    {
                        Latitude = float.Parse(x[0], CultureInfo.InvariantCulture),
                        Longitude = float.Parse(x[1], CultureInfo.InvariantCulture),
                        HousingMedianAge = float.Parse(x[2], CultureInfo.InvariantCulture),
                        TotalRooms = float.Parse(x[3], CultureInfo.InvariantCulture),
                        TotalBedrooms = float.Parse(x[4], CultureInfo.InvariantCulture),
                        Population = float.Parse(x[5], CultureInfo.InvariantCulture),
                        HouseHolds = float.Parse(x[6], CultureInfo.InvariantCulture),
                        MedianIncome = float.Parse(x[7], CultureInfo.InvariantCulture),
                        OceanProximity = x[8],
                        MedianHouseValue = float.Parse(x[9], CultureInfo.InvariantCulture)
                    })
                    .Take<HousingRow>(numMaxRecords);

                return records;
            }
        }
    }
}
