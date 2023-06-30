using AIaaS.WebAPI.Data;
using ConsoleAppTest;
using Microsoft.Data.Analysis;
using Microsoft.ML;
using Microsoft.ML.Transforms;
using static ConsoleAppTest.SalesPrediction;

namespace Regression_TaxiFarePrediction
{
    internal static class Program
    {

      
        //https://github.com/jwood803/MLNetExamples/blob/master/MLNetExamples/NullValues/ReplacedValues.cs
        static void Main(string[] args) //If args[0] == "svg" a vector-based chart will be created instead a .png chart
        {
            //var gen = new TaxiFarePricePrediction();
            //gen.Run();
            //var gen = new HousingPricePrediction();
            //var gen = new SalesPrediction();
            //var gen = new MLTests();
            //gen.Run();

            //ReplaceMissingValues();
            //var dbContext = new EfContext();

            var titanicPrediction = new TitanicPrediction();
            titanicPrediction.Run();

            var cardFraud = new CreditCardFraudDetection();
            cardFraud.Run();

            var hpp = new HousingPricePrediction();
            hpp.Run();

                DatasetTest();
            return;

            var mlContext = new MLContext();
            var save = true;
            if (save)
            {
                IDataView data = mlContext.Data.LoadFromTextFile<AdvertisingRow>("advertising.csv", hasHeader: true, separatorChar: ',');

                //var preview = data.Preview();
                using (FileStream stream = new FileStream("data.idv", FileMode.Create))
                {
                    mlContext.Data.SaveAsBinary(data, stream);
                    stream.Flush();
                }
            }
            else
            {
                var data = mlContext.Data.LoadFromBinary("data.idv");

                var preview = data.Preview();
            }

        }

        private static void DatasetTest()
        {            
            var dataFrame = DataFrame.LoadCsv("advertising.csv");
            var description= dataFrame.Description();
            var info=  dataFrame.Info();
        }

        private static void ReplaceMissingValues()
        {
            HomeData[] homeData = new HomeData[]{
                new (){NumberOfBedrooms=2f,Price=10f, Sales=100f},
                new (){NumberOfBedrooms=4f,Price=20f, Sales=150f},
                new (){NumberOfBedrooms=float.NaN,Price=float.NaN, Sales=200f}
            };

            var mlContext = new MLContext();
            var data = mlContext.Data.LoadFromEnumerable(homeData);

            // Define replacement estimator
            //var replacementEstimator = mlContext.Transforms.ReplaceMissingValues("Price", "Price", replacementMode: MissingValueReplacingEstimator.ReplacementMode.Mean);
            var replacementEstimator = mlContext.Transforms.CopyColumns(outputColumnName: "Label", "Sales")
                .Append(mlContext.Transforms.ReplaceMissingValues(outputColumnName: "NumberOfBedrooms", replacementMode: MissingValueReplacingEstimator.ReplacementMode.Mean))
                .Append(mlContext.Transforms.ReplaceMissingValues(outputColumnName: "Price", replacementMode: MissingValueReplacingEstimator.ReplacementMode.Mean));

            ITransformer replacementTransformer = replacementEstimator.Fit(data);

            IDataView transformedData = replacementTransformer.Transform(data);
            var preview = transformedData.Preview(10);
            var transformedDataAsEnumerable = mlContext.Data.CreateEnumerable<HomeData>(transformedData, false);
        }
    }

    public class HomeData
    {
        public float NumberOfBedrooms { get; set; }
        public float Price { get; set; }
        public float Sales { get; set; }
    }
}
