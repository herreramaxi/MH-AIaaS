using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Domain.Entities.enums;
using AIaaS.WebAPI.ExtensionMethods;
using AIaaS.WebAPI.Services;
using Ardalis.Result;
using Microsoft.ML;
using Microsoft.ML.Trainers;

namespace AIaaS.Application.Features.Workflows.Commands.Common.Operators
{
    [Operator("Train Model", OperatorType.Train, 4)]
    [OperatorParameter("Label", "The name of the label column", "text")]
    [OperatorParameter("Task", "Type of prediction or inference being made", "list")]
    [OperatorParameter("Trainer", "Trainer to perform the prediction or inference", "list")]
    public class TrainModelOperator : WorkflowOperatorAbstract
    {
        private string? _labelColumn;
        private string? _task;
        private string? _trainer;
        private MetricTypeEnum? _taskAsEnum;
        private const string FEATURE_COLUMN_NAME = "Features";
        public TrainModelOperator(IOperatorService operatorService) : base(operatorService)
        {
        }

        public override Task Hydrate(WorkflowContext mlContext, WorkflowNodeDto root)
        {
            _labelColumn = root.GetParameterValue("Label");
            _task = root.GetParameterValue("Task");
            _trainer = root.GetParameterValue("Trainer");

            if (Enum.TryParse(_task, out MetricTypeEnum metricType))
            {
                _taskAsEnum = metricType;
            }

            return Task.CompletedTask;
        }

        public override Result Validate(WorkflowContext mlContext, WorkflowNodeDto root)
        {
            if (root.Data.DatasetColumns is null || !root.Data.DatasetColumns.Any())
            {
                return Result.Error("No selected columns detected on pipeline, please select columns on dataset operator");
            }

            if (string.IsNullOrEmpty(_labelColumn))
            {
                return Result.Error("Please configure a label column");
            }

            if (string.IsNullOrEmpty(_task))
            {
                return Result.Error("Please select a ML task");
            }

            if (_taskAsEnum is null)
            {
                return Result.Error($"The ML task {_task} selected is invalid, please select a valid ML task from the operator configuration");
            }

            if (string.IsNullOrEmpty(_trainer))
            {
                return Result.Error("Please select a task trainer");
            }

            return Result.Success();
        }

        public override async Task<Result> Run(WorkflowContext context, WorkflowNodeDto root, CancellationToken cancellationToken)
        {
            context.Task = _taskAsEnum;

            if (context.ColumnSettings is null || !context.ColumnSettings.Any())
            {
                return Result.Error("Column settings not found, please select columns on dataset operator");
            }

            context.LabelColumn = _labelColumn;
            var features = context.ColumnSettings
                .Where(x => !x.ColumnName.Equals(_labelColumn, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => x.ColumnName)
                .ToArray();

            if (!features.Any())
            {
                return Result.Error("Feature columns not found, please select columns on dataset operator");
            }

            if (context.TrainingData is null)
            {
                return Result.Error("Training data not found, please ensure that there is a 'Split Data' operator correctly configured on the pipeline");
            }

            var mlContext = context.MLContext;
            var estimator = mlContext.Transforms.Concatenate(FEATURE_COLUMN_NAME, features);
            context.EstimatorChain = context.EstimatorChain.AppendEstimator(estimator);

            var trainer = GetTrainer(mlContext, _task, _trainer);
            if (trainer is null)
            {
                return Result.Error($"No trainer found for {_trainer}");
            }

            var trainingPipeline = context.EstimatorChain.Append(trainer);
            var trainedModel = trainingPipeline.Fit(context.TrainingData);
            context.TrainedModel = trainedModel;
            context.Trainer = trainer;

            using var stream = new MemoryStream();
            mlContext.Model.Save(trainedModel, context.TrainingData.Schema, stream);
            stream.Seek(0, SeekOrigin.Begin);

            await _operatorService.UpdateModel(context.Workflow, stream, cancellationToken);

            return Result.Success();
        }

        private IEstimator<ITransformer>? GetTrainer(MLContext mlContext, string task, string trainerName)
        {
            if (task == "Regression")
            {
                switch (trainerName)
                {
                    case "SdcaRegression": return mlContext.Regression.Trainers.Sdca(labelColumnName: _labelColumn, featureColumnName: FEATURE_COLUMN_NAME);
                    case "Ols": {
                            var options = new OlsTrainer.Options
                            {
                                LabelColumnName = nameof(_labelColumn),
                                FeatureColumnName = FEATURE_COLUMN_NAME,
                                // Larger values leads to smaller (closer to zero) model parameters.
                                L2Regularization = 0.1f,
                                // Whether to compute standard error and other statistics of model
                                // parameters.
                                CalculateStatistics = false
                            };

                            return mlContext.Regression.Trainers.Ols(labelColumnName: _labelColumn, featureColumnName: FEATURE_COLUMN_NAME); }
                    case "OnlineGradientDescent": return mlContext.Regression.Trainers.OnlineGradientDescent(labelColumnName: _labelColumn, featureColumnName: FEATURE_COLUMN_NAME);
                    default:
                        return mlContext.Regression.Trainers.Sdca(labelColumnName: _labelColumn, featureColumnName: FEATURE_COLUMN_NAME);
                }
            }
            else if (task == "BinaryClassification")
            {
                switch (trainerName)
                {
                    case "SdcaLogisticRegression": return mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: _labelColumn, featureColumnName: FEATURE_COLUMN_NAME);
                    case "LinearSvm": return mlContext.BinaryClassification.Trainers.LinearSvm(labelColumnName: _labelColumn, featureColumnName: FEATURE_COLUMN_NAME);
                    case "AveragedPerceptron": return mlContext.BinaryClassification.Trainers.AveragedPerceptron(labelColumnName: _labelColumn, featureColumnName: FEATURE_COLUMN_NAME);
                    case "FastTree": return mlContext.BinaryClassification.Trainers.FastTree(labelColumnName: _labelColumn, featureColumnName: FEATURE_COLUMN_NAME);
                    default:
                        return mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: _labelColumn, featureColumnName: FEATURE_COLUMN_NAME);
                }

            }
            else return null;
        }
    }
}
