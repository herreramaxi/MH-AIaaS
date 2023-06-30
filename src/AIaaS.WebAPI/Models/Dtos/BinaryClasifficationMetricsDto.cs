namespace AIaaS.WebAPI.Models.Dtos
{
    public class BinaryClasifficationMetricsDto
    {
        public string LogLossReduction { get; set; }
        public string Accuracy { get; set; }
        public string LogLoss { get; set; }
        public string NegativeRecall { get; set; }
        public string PositiveRecall { get; set; }
        public string AreaUnderPrecisionRecallCurve { get; set; }
        public string AreaUnderRocCurve { get; set; }
        public string Entropy { get; set; }
        public string F1Score { get; set; }
        public string Task { get; set; }
    }
}
