using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;

namespace AIaaS.WebAPI.Models
{
    public class WorkflowContext
    {
        public MLContext MLContext { get; set; }
        public IDataView DataView { get; set; }
        public IDataView TrainingData { get; internal set; }
        public IDataView TestData { get; internal set; }
        public ITransformer TrainedModel { get; internal set; }
        public SdcaRegressionTrainer Trainer { get; internal set; }
        public Dataset Dataset { get; internal set; }
        public Workflow Workflow { get; internal set; }
        public IEnumerable<ColumnSetting> ColumnSettings { get; internal set; }
    }
}
