using AIaaS.Domain.Entities;
using AIaaS.Domain.Entities.enums;
using Microsoft.ML;
using Microsoft.ML.Trainers;

namespace AIaaS.Application.Common.Models
{
    public class WorkflowContext
    {
        public MLContext MLContext { get; set; }
        public IDataView DataView { get; set; }
        public IDataView TrainingData { get; set; }
        public IDataView TestData { get; set; }
        public ITransformer TrainedModel { get; set; }
        public IEstimator<ITransformer> Trainer { get; set; }
        public Dataset? Dataset { get; set; }
        public Workflow Workflow { get; set; }
        public IEnumerable<ColumnSetting>? ColumnSettings { get; set; }
        public InputOutputColumnPair[]? InputOutputColumns { get; set; }
        public IEstimator<ITransformer> EstimatorChain { get; set; }
        public string? LabelColumn { get; internal set; }
        public bool RunWorkflow { get; internal set; }
        public MetricTypeEnum? Task { get; internal set; }
        public string MetricsSerialized { get; internal set; }
    }
}
