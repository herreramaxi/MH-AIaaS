using Microsoft.ML.Data;

namespace Regression_TaxiFarePrediction.DataStructures
{
    public class TaxiTripFarePrediction
    {
        [ColumnName("Score")]
        public float FareAmount;
    }

    public class HousingRowPrediction
    {
        [ColumnName("Score")]
        public float MedianHouseValue;
    }
}