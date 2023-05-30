using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Models.enums;
using Microsoft.ML;
using Microsoft.ML.Transforms;

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
            try
            {
                _dbContext.Entry(context.Workflow).Reference(x => x.MLModel).LoadAsync();

                var labelColumn = root.Data.Config.FirstOrDefault(x => x.Name.Equals("Label")).Value.ToString();
                var features = context.ColumnSettings.Where(x => !x.ColumnName.Equals(labelColumn, StringComparison.InvariantCultureIgnoreCase)).Select(x => x.ColumnName).ToArray();
                var mlContext = context.MLContext;

                var inputOutputColumns = context.ColumnSettings.Select(x => new InputOutputColumnPair(x.ColumnName, x.ColumnName)).ToArray();
                var dataProcessPipeline = mlContext.Transforms.NormalizeMeanVariance(inputOutputColumns);

                var est = dataProcessPipeline.Append(mlContext.Transforms.Concatenate("Features", features));


                //var dataProcessPipeline = mlContext.Transforms.NormalizeMeanVariance(outputColumnName: "TV")
                //        .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: "Radio"))
                //        .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: "Newspaper"))
                //        .Append(mlContext.Transforms.Concatenate("Features", "TV", "Radio", "Newspaper"));

                var trainer = mlContext.Regression.Trainers.Sdca(labelColumnName: labelColumn, featureColumnName: "Features");
                var trainingPipeline = est.Append(trainer);
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
                else {

                    context.Workflow.MLModel.Data = stream.ToArray();
                    context.Workflow.MLModel.Size = stream.Length;
                }

                await _dbContext.SaveChangesAsync();
                //mlContext.Model.Save(trainedModel, context.TrainingData.Schema, "model.zip");
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }
    }
}
