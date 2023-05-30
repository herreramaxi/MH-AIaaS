using Common;
using Microsoft.Data.Analysis;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using static ConsoleAppTest.SalesPrediction;

namespace ConsoleAppTest
{
    public class MLTests
    {
        private static string Dataset = "advertisingWithMissingValues.csv";
        private static string ModelPath = $"advertising.zip";
        public void Run()
        {  //Create ML Context with seed for repeatable/deterministic results
            MLContext mlContext = new MLContext();

            // Create, Train, Evaluate and Save a model
            BuildTrainEvaluateAndSaveModel(mlContext);

            // Make a single test prediction loding the model from .ZIP file
            //TestSinglePrediction(mlContext);

            //// Paint regression distribution chart for a number of elements read from a Test DataSet file
            //PlotRegressionChart(mlContext, Dataset, 50, new string[] { });

            Console.WriteLine("Press any key to exit..");
            Console.ReadLine();
        }

        private static ITransformer BuildTrainEvaluateAndSaveModel(MLContext mlContext)
        {
            TextLoader.Column[] columns = new TextLoader.Column[] {
                new TextLoader.Column("Tv",DataKind.Single,0),
            new TextLoader.Column("Radio",DataKind.Single,1),
            new TextLoader.Column("Newspaper",DataKind.Single,2),
             new TextLoader.Column("Sales",DataKind.Single,3),
            };

            var loaderOptions = new TextLoader.Options()
            {
                Columns = columns,
                HasHeader = true,
                Separators = new[] { ',' },
                MissingRealsAsNaNs = true
            };

            var textLoader = mlContext.Data.CreateTextLoader(loaderOptions);
            var data = textLoader.Load(Dataset);

            //.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(HousingRow.MedianHouseValue))
            //      .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(HousingRow.Longitude)))
            //       .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(HousingRow.Latitude)))
            //          .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(HousingRow.HousingMedianAge)))
            //          .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(HousingRow.TotalRooms)))
            //          .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(HousingRow.TotalBedrooms)))
            //          .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(HousingRow.Population)))
            //          .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(HousingRow.HouseHolds)))
            //          .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(HousingRow.MedianIncome)))
            //          .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "OceanProximityEncoded", inputColumnName: nameof(HousingRow.OceanProximity)))

            //       .Append(mlContext.Transforms.Concatenate("Features",
            //       nameof(HousingRow.Longitude),
            //       nameof(HousingRow.Latitude),
            //       nameof(HousingRow.HousingMedianAge),
            //       nameof(HousingRow.TotalRooms),
            //       nameof(HousingRow.TotalBedrooms),
            //       nameof(HousingRow.Population),
            //       nameof(HousingRow.HouseHolds),
            //       nameof(HousingRow.MedianIncome),
            //       "OceanProximityEncoded"
            //       ));


            var replacementEstimator = mlContext.Transforms
            .CopyColumns(outputColumnName: "Label", inputColumnName: "Sales")
            .Append(mlContext.Transforms.ReplaceMissingValues(outputColumnName: "Tv", replacementMode: MissingValueReplacingEstimator.ReplacementMode.Mean))
            .Append(mlContext.Transforms.ReplaceMissingValues(outputColumnName: "Radio", replacementMode: MissingValueReplacingEstimator.ReplacementMode.Mean))
            .Append(mlContext.Transforms.ReplaceMissingValues(outputColumnName: "Newspaper", replacementMode: MissingValueReplacingEstimator.ReplacementMode.Mean))
            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: "Tv"))
            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: "Radio"))
            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: "Newspaper"));

            ITransformer replacementTransformer = replacementEstimator.Fit(data);
            IDataView transformedData = replacementTransformer.Transform(data);
            var preview2 = transformedData.Preview();

            var defaultRowEnumerable = mlContext.Data.CreateEnumerable<HousingRow>(transformedData, false);

            return null;
        }
    }

    public class HousingRow
    {
        public float Tv { get; set; }
        public float Radio { get; set; }
        public float Newspaper { get; set; }
        public float Sales { get; set; }
    }
}
