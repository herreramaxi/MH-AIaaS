using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ConsoleAppTest.HousingPricePrediction;

namespace ConsoleAppTest
{
    public class HousingPricePrediction2
    {
        private static string TrainDataRelativePath = $"housing.csv";
        private static string TestDataRelativePath = $"housing.csv";

        private static string BaseModelsRelativePath = @"../../../../MLModels";
        private static string ModelRelativePath = $"Housing.zip";
        private static readonly Dictionary<int, int> MISSING_INDEXES = new Dictionary<int, int>();
        internal class MissingData : HousingRow
        {
            public bool[] MissingValues { get; set; }

        }
        internal class ReplacedValues : HousingRow
        {
            public float[] NewValues { get; set; }
        }

        public void Run() {
            var context = new MLContext();

            var data = context.Data.LoadFromTextFile<HousingRow>("housing.csv", hasHeader: true, separatorChar: ',');

            var columns = data.Schema
                .Select(col => col.Name)
                .Where(colName => colName != "Label" && colName != "OceanProximity")
                .ToArray();

            // Indicate missing values
            var nullTransform = context.Transforms.Concatenate("Features", columns)
                .Append(context.Transforms.IndicateMissingValues("MissingValues", "Features"));

            var nullValues = nullTransform.Fit(data).Transform(data);

            var nullData = context.Data.CreateEnumerable<MissingData>(nullValues,
                reuseRowObject: false).ToArray();

            for (int i = 0; i < nullData.Length; i++)
            {
                if (nullData[i].MissingValues.Any(a => a == true))
                {
                    var missingIndexes = nullData[i].MissingValues.Select((v, idx) => v ? idx : -1)
                        .Where(idx => idx != -1)
                        .ToArray();

                    foreach (var index in missingIndexes)
                    {
                        var feature = columns[index];
                        MISSING_INDEXES.Add(i, index);

                        Console.WriteLine($"Feature {feature} in row {i + 1} has missing value");
                    }
                }
            }

            // Replace missing values
            var replaceTransform = context.Transforms.Concatenate("Features", columns)
                .Append(context.Transforms.ReplaceMissingValues("NewValues", "Features",
                    Microsoft.ML.Transforms.MissingValueReplacingEstimator.ReplacementMode.Mean));

            var replacedValues = replaceTransform.Fit(data).Transform(data);

            var replacedData = context.Data.CreateEnumerable<ReplacedValues>(replacedValues,
                reuseRowObject: false).ToArray();

            for (int i = 0; i < replacedData.Count(); i++)
            {
                foreach (var index in MISSING_INDEXES)
                {
                    if (i == index.Key)
                    {
                        Console.WriteLine($"New value - {replacedData[i].NewValues[index.Value]}");
                    }
                }
            }
        }
    }
}
