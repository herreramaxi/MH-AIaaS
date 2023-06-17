namespace AIaaS.WebAPI.Models.Dtos
{
    public class BinaryClasifficationMetricsDto
    {
        public double LogLossReduction { get; set; }
        public double Accuracy { get; set; }
        public double LogLoss { get; set; }
        public double NegativeRecall { get; set; }
        public double PositiveRecall { get; set; }
        public double AreaUnderPrecisionRecallCurve { get; set; }
        public double AreaUnderRocCurve { get; set; }
        public double Entropy { get; set; }
        public double F1Score { get; set; }
        public string Task { get; set; }
    }
}
