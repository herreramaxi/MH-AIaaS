using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Entities.enums;
using CleanArchitecture.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;

namespace AIaaS.Application.Common.Models.Operators
{
    [Operator("Train Model", OperatorType.Train, 4)]
    [OperatorParameter("Label", "The name of the label column", "text")]
    [OperatorParameter("Task", "Type of prediction or inference being made", "list")]
    [OperatorParameter("Trainer", "Trainer to perform the prediction or inference", "list")]
    public class TrainModelOperator : WorkflowOperatorAbstract
    {
        private readonly IApplicationDbContext _dbContext;
        private string? _labelColumn;
        private string? _task;
        private string? _trainer;

        public TrainModelOperator(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override Task Hydrate(WorkflowContext mlContext, WorkflowNodeDto root)
        {
            _labelColumn = root.GetParameterValue("Label");
            _task = root.GetParameterValue("Task");
            _trainer = root.GetParameterValue("Trainer");

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

            if (string.IsNullOrEmpty(_task))
            {
                root.SetAsFailed("Please select a ML task");
                return false;
            }

            if (string.IsNullOrEmpty(_trainer))
            {
                root.SetAsFailed("Please select a task trainer");
                return false;
            }

            return true;
        }

        public override async Task Run(WorkflowContext context, WorkflowNodeDto root)
        {
            context.Task = _task;

            if (context.ColumnSettings is null || !context.ColumnSettings.Any())
            {
                root.SetAsFailed("Column settings not found, please select columns on dataset operator");
                return;
            }
            
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

            var trainer = GetTrainer(mlContext,_task, _trainer);
            if (trainer is null) {
                root.SetAsFailed($"No trainer found for {_trainer}");
                return;
            }

            var trainingPipeline = context.EstimatorChain.Append(trainer);
            var trainedModel = trainingPipeline.Fit(context.TrainingData);
            context.TrainedModel = trainedModel;
            context.Trainer = trainer;

            using var stream = new MemoryStream();
            mlContext.Model.Save(trainedModel, context.TrainingData.Schema, stream);
            stream.Seek(0, SeekOrigin.Begin);
                  
            if (context.Workflow.MLModel is null)
            {
                var mlModel  = new MLModel
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
        }

        private IEstimator<ITransformer>? GetTrainer(MLContext mlContext ,string task, string trainerName)
        {         
            if (task == "Regression")
            {
                switch (trainerName)
                {
                    case "SdcaRegression": return mlContext.Regression.Trainers.Sdca(labelColumnName: _labelColumn, featureColumnName: "Features");
                    case "Ols": return mlContext.Regression.Trainers.Ols(labelColumnName: _labelColumn, featureColumnName: "Features");
                    case "OnlineGradientDescent": return mlContext.Regression.Trainers.OnlineGradientDescent(labelColumnName: _labelColumn, featureColumnName: "Features");
                    default:
                        return mlContext.Regression.Trainers.Sdca(labelColumnName: _labelColumn, featureColumnName: "Features");
                }
            }
            else if (task == "BinaryClassification")
            {
                switch (trainerName)
                {
                    case "SdcaLogisticRegression": return mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: _labelColumn, featureColumnName: "Features");
                    case "LinearSvm": return mlContext.BinaryClassification.Trainers.LinearSvm(labelColumnName: _labelColumn, featureColumnName: "Features");
                    case "AveragedPerceptron": return mlContext.BinaryClassification.Trainers.AveragedPerceptron(labelColumnName: _labelColumn, featureColumnName: "Features");
                    case "FastTree": return mlContext.BinaryClassification.Trainers.FastTree(labelColumnName: _labelColumn, featureColumnName: "Features");
                    default:
                        return mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: _labelColumn, featureColumnName: "Features");
                }

            }
            else return null;
        }
    }
}
