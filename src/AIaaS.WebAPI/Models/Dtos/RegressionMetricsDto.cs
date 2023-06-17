namespace AIaaS.WebAPI.Models.Dtos
{
    public class RegressionMetricsDto
    {
        public double MeanAbsoluteError { get; set; }
        public double MeanSquaredError { get; set; }
        public double RootMeanSquaredError { get; set; }
        public double LossFunction { get; set; }
        public double RSquared { get; set; }
        public string Task { get; set; }
    }
}
