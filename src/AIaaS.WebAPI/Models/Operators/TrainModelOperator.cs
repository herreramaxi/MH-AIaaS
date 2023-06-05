using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Models.enums;
using Microsoft.ML;

namespace AIaaS.WebAPI.Models.Operators
{
    [Operator("Train Model", OperatorType.Train, 4)]

    [OperatorParameter("Label", "The name of the label column", "text")]
    public class TrainModelOperator : WorkflowOperatorAbstract
    {
        private readonly EfContext _dbContext;

        public TrainModelOperator(EfContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task Execute(WorkflowContext context, Dtos.WorkflowNodeDto root)
        {
            var labelColumn = root.GetParameterValue("Label");

            if (string.IsNullOrEmpty(labelColumn))
            {
                root.Error("Please configure a label column");
                return;
            }

            if (!context.ColumnSettings.Any())
            {
                root.Error("No columns were found, please select columns on dataset operator");
                return;
            }

            await _dbContext.Entry(context.Workflow).Reference(x => x.MLModel).LoadAsync();

            context.LabelColumn = labelColumn;
            var features = context.ColumnSettings.Where(x => !x.ColumnName.Equals(labelColumn, StringComparison.InvariantCultureIgnoreCase)).Select(x => x.ColumnName).ToArray();
            var mlContext = context.MLContext;

            context.EstimatorChain = context.EstimatorChain is not null ?
                context.EstimatorChain.Append(mlContext.Transforms.Concatenate("Features", features)) :
                mlContext.Transforms.Concatenate("Features", features);

            var trainer = mlContext.Regression.Trainers.Sdca(labelColumnName: labelColumn, featureColumnName: "Features");
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

            root.Success();
        }
    }
}
