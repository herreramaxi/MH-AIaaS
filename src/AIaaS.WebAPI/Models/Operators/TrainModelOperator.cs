using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Models.Dtos;
using AIaaS.WebAPI.Models.enums;
using Microsoft.ML;

namespace AIaaS.WebAPI.Models.Operators
{
    [Operator("Train Model", OperatorType.Train, 4)]

    [OperatorParameter("Label", "The name of the label column", "text")]
    public class TrainModelOperator : WorkflowOperatorAbstract
    {
        private readonly EfContext _dbContext;
        private string? _labelColumn;

        public TrainModelOperator(EfContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override Task Hydrate(WorkflowContext mlContext, WorkflowNodeDto root)
        {
            _labelColumn = root.GetParameterValue("Label");

            return Task.CompletedTask;
        }

        public override bool Validate(WorkflowContext mlContext, WorkflowNodeDto root)
        {
            if (root.Data?.DatasetColumns is null || !root.Data.DatasetColumns.Any())
            {
                root.SetAsFailed("No selected columns detected on pipeline, please select columns on dataset operator");
                return false;
            }

            if (string.IsNullOrEmpty(_labelColumn))
            {
                root.SetAsFailed("Please configure a label column");
                return false;
            }

            return true;
        }

        public override async Task Run(WorkflowContext context, WorkflowNodeDto root)
        {
            if (context.ColumnSettings is null || !context.ColumnSettings.Any())
            {
                root.SetAsFailed("Column settings not found, please select columns on dataset operator");
                return;
            }

            await _dbContext.Entry(context.Workflow).Reference(x => x.MLModel).LoadAsync();

            context.LabelColumn = _labelColumn;
            var features = context.ColumnSettings
                .Where(x => !x.ColumnName.Equals(_labelColumn, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => x.ColumnName)
                .ToArray();

            if (!features.Any())
            {
                root.SetAsFailed("Feature columns not found, please select columns on dataset operator");
                return;
            }

            if (context.TrainingData is null)
            {
                root.SetAsFailed("Training data not found, please ensure that there is a 'Split Data' operator correctly configured on the pipeline");
                return;
            }

            var mlContext = context.MLContext;
            context.EstimatorChain = context.EstimatorChain is not null ?
                context.EstimatorChain.Append(mlContext.Transforms.Concatenate("Features", features)) :
                mlContext.Transforms.Concatenate("Features", features);

            var trainer = mlContext.Regression.Trainers.Sdca(labelColumnName: _labelColumn, featureColumnName: "Features");
            var trainingPipeline = context.EstimatorChain.Append(trainer);
            var trainedModel = trainingPipeline.Fit(context.TrainingData);
            context.TrainedModel = trainedModel;
            context.Trainer = trainer;

            using var stream = new MemoryStream();
            mlContext.Model.Save(trainedModel, context.TrainingData.Schema, stream);
            stream.Seek(0, SeekOrigin.Begin);

            if (context.Workflow.MLModel is null)
            {
                var mlModel = new MLModel
                {
                    Data = stream.ToArray(),
                    Size = stream.Length,
                    Workflow = context.Workflow
                };

                await _dbContext.MLModels.AddAsync(mlModel);
            }
            else
            {
                context.Workflow.MLModel.Data = stream.ToArray();
                context.Workflow.MLModel.Size = stream.Length;
            }

            await _dbContext.SaveChangesAsync();
            //mlContext.Model.Save(trainedModel, context.TrainingData.Schema, "model.zip");
        }
    }
}
