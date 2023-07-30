using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Domain.Entities;
using AIaaS.WebAPI.Interfaces;
using AIaaS.WebAPI.Services;
using Ardalis.Result;
using Microsoft.ML;
using System.Diagnostics;

namespace AIaaS.Application.Features.Workflows.Commands.Common.Operators
{
    public abstract class WorkflowOperatorAbstract : IWorkflowOperator
    {
        protected readonly IOperatorService _operatorService;

        public string Name { get; set; }
        public string Type { get; set; }


        public WorkflowOperatorAbstract(IOperatorService operatorService)
        {
            var operatorAttribute = (OperatorAttribute?)GetType().GetCustomAttributes(typeof(OperatorAttribute), false).FirstOrDefault();
            if (operatorAttribute is null) throw new Exception($"OperatorAttribute not found on type {GetType().FullName}");

            Name = operatorAttribute.Name;
            Type = operatorAttribute.Type;
            _operatorService = operatorService;
        }

        public virtual void Preprocessing(WorkflowContext context, WorkflowNodeDto parent)
        {
            parent.Data.Status = null;
            parent.Data.StatusDetail = null;
            parent.Data.DatasetColumns = null;
        }

        public abstract Task Hydrate(WorkflowContext context, WorkflowNodeDto root);
        public abstract Task<Result> Run(WorkflowContext context, WorkflowNodeDto root, CancellationToken cancellationToken);
        public abstract Result Validate(WorkflowContext context, WorkflowNodeDto root);
        public virtual void PropagateDatasetColumns(WorkflowContext context, WorkflowNodeDto root)
        {
            var parentDatasetColumns = root.Parent?.Data.DatasetColumns;

            root.SetDatasetColumns(parentDatasetColumns);
        }

        virtual public async Task<Result> GenerateOuput(WorkflowContext context, WorkflowNodeDto root, CancellationToken cancellationToken)
        {
            if (context.EstimatorChain is null) return Result.Success();

            var transformer = context.EstimatorChain.Fit(context.DataView);
            var dataview = transformer.Transform(context.DataView);
            using var stream = new MemoryStream();
            context.MLContext.Data.SaveAsBinary(dataview, stream);
            stream.Seek(0, SeekOrigin.Begin);

            var result = await _operatorService.GenerateWorkflowDataView(context.Workflow, root.Data.NodeGuid, root.Type, stream, cancellationToken);
            if (!result.IsSuccess)
            {
                return Result.Error(result.Errors.FirstOrDefault());
            }

            root.Data.Parameters = new Dictionary<string, object>
            {
                { "WorkflowDataViewId", result.Value.Id }
            };

            return Result.Success();
        }
    }
}
