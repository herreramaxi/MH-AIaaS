namespace AIaaS.Application.Common.Models.Dtos
{
    public class RegressionMetricsDto
    {
        public string MeanAbsoluteError { get; set; }
        public string MeanSquaredError { get; set; }
        public string RootMeanSquaredError { get; set; }
        public string LossFunction { get; set; }
        public string RSquared { get; set; }
        public string Task { get; set; }
    }
}
