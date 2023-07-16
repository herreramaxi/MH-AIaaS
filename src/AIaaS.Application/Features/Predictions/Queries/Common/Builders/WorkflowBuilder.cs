using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Domain.Entities.enums;
using AIaaS.WebAPI.ExtensionMethods;
using Ardalis.Result;
using Microsoft.ML;
using System.Text.Json;

namespace AIaaS.Application.Features.Predictions.Queries.Common.Builders
{
    [PredictServiceParameterBuilder(PredictServiceBuilderType.Workflow, 4)]
    public class WorkflowBuilder : PredictServiceParameterBuilderAbstract
    {
        public override async Task<Result<PredictionParameter>> Build(PredictionParameter parameter)
        {
            if (parameter.Endpoint?.MLModel?.Workflow is null)
            {
                return Result.NotFound("Workflow not found");
            }

            parameter.Workflow = parameter.Endpoint.MLModel.Workflow;

            if (string.IsNullOrEmpty(parameter.Workflow?.Data))
            {
                return Result.NotFound("Workflow Data not found");
            }

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var workflowGraphDto = JsonSerializer.Deserialize<WorkflowGraphDto>(parameter.Endpoint.MLModel.Workflow.Data, jsonOptions);

            if (workflowGraphDto?.Root is null)
            {
                return Result.Error("Workflow empty after deserialization");
            }

            var nodes = workflowGraphDto.Root.ToList();
            var trainingNode = nodes.FirstOrDefault(x => x.Type.Equals(OperatorType.Train.ToString(), StringComparison.InvariantCultureIgnoreCase));
            var datasetNode = nodes.FirstOrDefault(x => x.Type.Equals(OperatorType.Dataset.ToString(), StringComparison.InvariantCultureIgnoreCase));

            if (datasetNode is null)
            {
                return Result.Error("Dataset operator not found");
            }

            var selectedColumns = datasetNode.GetParameterValue<string, IList<string>>("SelectedColumns", x => JsonSerializer.Deserialize<IList<string>>(x));

            //TODO: maybe last columns from last node?
            if (selectedColumns is null || !selectedColumns.Any())
            {
                return Result.Error("No selected columns found on Dataset operator");
            }

            parameter.SelectedColumns = selectedColumns;

            if (trainingNode is null)
            {
                return Result.Error("Train Model operator not found");
            }

            var task = trainingNode.GetParameterValue("Task");
            if (string.IsNullOrEmpty(task))
            {
                return Result.Error("Task not found");
            };

            if (!task.Equals("Regression", StringComparison.InvariantCultureIgnoreCase) && !task.Equals("BinaryClassification", StringComparison.InvariantCultureIgnoreCase))
            {
                return Result.Error($"Prediction logic not implemented for task: {task}");
            };

            parameter.Task = task;

            var label = trainingNode.GetParameterValue("Label");

            if (string.IsNullOrEmpty(label))
            {
                return Result.Error("Label not found");
            };

            parameter.Label = label;

            var column = parameter.InputSchema.GetColumnOrNull(label);

            if (column is null)
            {
                return Result.Error("Label column not found");
            }

            parameter.LabelColumn = (DataViewSchema.Column)column;

            return Result.Success(parameter);
        }
    }
}
