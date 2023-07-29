﻿using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Domain.Entities.enums;
using AIaaS.WebAPI.ExtensionMethods;
using AIaaS.WebAPI.Services;
using Ardalis.Result;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Text.Json;

namespace AIaaS.Application.Features.Workflows.Commands.Common.Operators
{
    [Operator("Edit Dataset", OperatorType.EditDataset, 6)]
    [OperatorParameter("DataType", "Selected columns to be converted", "list")]
    [OperatorParameter("Categorical", "Selected columns to be converted", "list")]
    [OperatorParameter("SelectedColumns", "Selected columns to be converted", "list")]
    public class EditDatasetOPerator : WorkflowOperatorAbstract
    {
        private string? _dataType;
        private string? _categorical;
        private IList<string>? _selectedColumns;

        public EditDatasetOPerator(IOperatorService operatorService) : base(operatorService)
        {
        }

        public override Task Hydrate(WorkflowContext context, WorkflowNodeDto root)
        {
            _dataType = root.GetParameterValue("DataType");
            _categorical = root.GetParameterValue("Categorical");
            _selectedColumns = root.GetParameterValue<string, IList<string>>("SelectedColumns", x => JsonSerializer.Deserialize<IList<string>>(x));
            return Task.CompletedTask;
        }

        public override Result Validate(WorkflowContext context, WorkflowNodeDto root)
        {
            if (_selectedColumns is null || !_selectedColumns.Any())
            {
                return Result.Error("Please select columns to be converted");
            }

            if (string.IsNullOrEmpty(_dataType) && string.IsNullOrEmpty(_categorical))
            {
                return Result.Error("At least you must select a data type categorical conversion");
            }

            return Result.Success();
        }

        public override async Task<Result> Run(WorkflowContext context, WorkflowNodeDto root, CancellationToken cancellationToken)
        {
            if (_selectedColumns is null || !_selectedColumns.Any() || context.InputOutputColumns is null || !context.InputOutputColumns.Any())
            {
                return Result.Error("Please ensure the operator is correctly configured");
            }

            var mlContext = context.MLContext;
            var columnsToBeConverted = context.InputOutputColumns.Where(x => _selectedColumns.Contains(x.InputColumnName, StringComparer.InvariantCultureIgnoreCase)).ToArray();

            if (!string.IsNullOrEmpty(_dataType))
            {
                var converted = Enum.TryParse<DataKind>(_dataType, out var dataKind);

                if (!converted)
                {
                    return Result.Error("Wrong value from data type");
                }

                var estimator = mlContext.Transforms.Conversion.ConvertType(columnsToBeConverted, dataKind);
                context.EstimatorChain = context.EstimatorChain.AppendEstimator(estimator);
            }

            if (!string.IsNullOrEmpty(_categorical))
            {
                var estimator = GetCategoricalEstimator(mlContext, columnsToBeConverted);

                if (estimator is null)
                {
                    return Result.Error("Wrong value from categorical type");
                }

                context.EstimatorChain = context.EstimatorChain.AppendEstimator(estimator);
            }

            return Result.Success();
        }

        private IEstimator<ITransformer>? GetCategoricalEstimator(MLContext mlContext, InputOutputColumnPair[] columnsToBeConverted)
        {
            switch (_categorical)
            {
                case "OneHotEncoding": return mlContext.Transforms.Categorical.OneHotEncoding(columnsToBeConverted);
                case "OneHotHashEncoding": return mlContext.Transforms.Categorical.OneHotHashEncoding(columnsToBeConverted);
                default:
                    return null;
            }
        }
    }
}
