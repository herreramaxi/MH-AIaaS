﻿using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Domain.Entities.enums;
using Microsoft.ML;
using Microsoft.ML.Transforms;
using System.Text.Json;

namespace AIaaS.Application.Common.Models.Operators
{
    [Operator("Normalize", OperatorType.Normalize, 7)]
    [OperatorParameter("Normalization mode", "Normalization mode to be applied", "list")]
    [OperatorParameter("SelectedColumns", "Columns to be normalized", "list")]
    public class NormalizeOperator : WorkflowOperatorAbstract
    {
        private string? _normalizationMode;
        private IList<string>? _selectedColumns;

        public override Task Hydrate(WorkflowContext mlContext, WorkflowNodeDto root)
        {
            _normalizationMode = root.GetParameterValue("Normalization mode");

            _selectedColumns = root.GetParameterValue<string, IList<string>>("SelectedColumns", x => JsonSerializer.Deserialize<IList<string>>(x));

            return Task.CompletedTask;
        }

        public override bool Validate(WorkflowContext mlContext, WorkflowNodeDto root)
        {
            if (string.IsNullOrEmpty(_normalizationMode))
            {
                root.SetAsFailed("Normalization mode not selected, please select a normalization mode");
                return false;
            }

            if (root.Data?.DatasetColumns is null || !root.Data.DatasetColumns.Any())
            {
                root.SetAsFailed("No selected columns detected on pipeline, please select columns on dataset operator");
                return false;
            }

            if (_selectedColumns is null || !_selectedColumns.Any())
            {
                root.SetAsFailed("No selected columns detected on operator, please select any column to be normalized");
                return false;
            }

            return true;
        }

        public override Task Run(WorkflowContext context, WorkflowNodeDto root)
        {
            if (context.InputOutputColumns is null || !context.InputOutputColumns.Any())
            {
                root.SetAsFailed("No selected columns detected on pipeline, please select columns on dataset operator");
                return Task.CompletedTask;
            }

            if (string.IsNullOrEmpty(_normalizationMode) || _selectedColumns is null || !_selectedColumns.Any())
            {
                root.SetAsFailed("Please verify the operator is correctly configured");
                return Task.CompletedTask;
            }

            var mlContext = context.MLContext;
            var selectedColumns = context.InputOutputColumns
                .Where(x => _selectedColumns.Contains(x.InputColumnName, StringComparer.InvariantCultureIgnoreCase))
                .ToArray();

            var estimator = GetNormalizingEstimator(context, selectedColumns);

            context.EstimatorChain = context.EstimatorChain is not null ?
                context.EstimatorChain.Append(estimator) :
                estimator;

            return Task.CompletedTask;
        }


        private NormalizingEstimator GetNormalizingEstimator(WorkflowContext context, InputOutputColumnPair[] selectedColumns)
        {
            _ = Enum.TryParse<NormalizationTypeEnum>(_normalizationMode, out var normalizer);

            return normalizer switch
            {
                NormalizationTypeEnum.MinMax => context.MLContext.Transforms.NormalizeMinMax(selectedColumns),
                NormalizationTypeEnum.Binning => context.MLContext.Transforms.NormalizeBinning(selectedColumns),
                NormalizationTypeEnum.LogMeanVariance => context.MLContext.Transforms.NormalizeLogMeanVariance(selectedColumns),
                NormalizationTypeEnum.RobustScaling => context.MLContext.Transforms.NormalizeRobustScaling(selectedColumns),
                _ => context.MLContext.Transforms.NormalizeMeanVariance(selectedColumns),
            };
        }
    }
}