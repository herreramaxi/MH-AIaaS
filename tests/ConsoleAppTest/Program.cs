using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

using Common;
using ConsoleAppTest;
using Microsoft.ML;
using Microsoft.ML.Data;

using PLplot;

using Regression_TaxiFarePrediction.DataStructures;

using static Microsoft.ML.Transforms.NormalizingEstimator;

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
            var gen = new SalesPrediction();
            gen.Run();
        }
    }
}
