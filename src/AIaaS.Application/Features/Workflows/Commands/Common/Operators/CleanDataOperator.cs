using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Domain.Entities.enums;
using AIaaS.WebAPI.ExtensionMethods;
using AIaaS.WebAPI.Services;
using Ardalis.Result;
using Microsoft.ML;
using System.Text.Json;

namespace AIaaS.Application.Features.Workflows.Commands.Common.Operators
{
    [Operator("Clean Data", OperatorType.Clean, 2)]
    [OperatorParameter("Cleaning mode", "Cleaning mode to be applied", "list")]
    [OperatorParameter("SelectedColumns", "Columns to be cleaned", "list")]
    public class CleanDataOperator : WorkflowOperatorAbstract
    {
        private string? _cleanMode;
        private IList<string>? _selectedColumns;

        public CleanDataOperator(IOperatorService operatorService) : base(operatorService)
        {
        }

        public override Task Hydrate(WorkflowContext mlContext, WorkflowNodeDto root)
        {
            _cleanMode = root.GetParameterValue("Cleaning mode");
            _selectedColumns = root.GetParameterValue<string, IList<string>>("SelectedColumns", x => JsonSerializer.Deserialize<IList<string>>(x));

            return Task.CompletedTask;
        }

        public override Result Validate(WorkflowContext context, WorkflowNodeDto root)
        {
            if (string.IsNullOrEmpty(_cleanMode))
            {
                return Result.Error("Clean mode not selected, please select a clean mode");
            }

            if (root.Data?.DatasetColumns is null || !root.Data.DatasetColumns.Any())
            {
                return Result.Error("No selected columns detected on pipeline, please select columns on dataset operator");
            }

            if (_selectedColumns is null || !_selectedColumns.Any())
            {
                return Result.Error("No selected columns detected on operator, please select any column to be cleaned");
            }

            return Result.Success();
        }

        public override async Task<Result> Run(WorkflowContext context, WorkflowNodeDto root, CancellationToken cancellationToken)
        {
            if (context.InputOutputColumns is null || !context.InputOutputColumns.Any())
            {
                return Result.Error("No selected columns detected on pipeline, please select columns on dataset operator");
            }

            if (_cleanMode is null || _selectedColumns is null || !_selectedColumns.Any())
            {
                return Result.Error("Please verify the operator is correctly configured");
            }

            var mlContext = context.MLContext;
            var selectedColumns = context.InputOutputColumns
                .Where(x => _selectedColumns.Contains(x.InputColumnName, StringComparer.InvariantCultureIgnoreCase))
                .ToArray();

            if (_cleanMode.Equals("RemoveRow"))
            {
                //TODO: Implement
                throw new NotImplementedException("Remove row not implemented");
            }
            else
            {
                Enum.TryParse<Microsoft.ML.Transforms.MissingValueReplacingEstimator.ReplacementMode>(_cleanMode, out var replacementMode);
                var estimator = mlContext.Transforms.ReplaceMissingValues(selectedColumns, replacementMode);
                context.EstimatorChain = context.EstimatorChain.AppendEstimator(estimator);
            }

            return Result.Success();
        }
    }
}

