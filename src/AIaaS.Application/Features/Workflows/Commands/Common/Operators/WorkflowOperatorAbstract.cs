using AIaaS.Application.Common.Models;
using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.WebAPI.Interfaces;
using AIaaS.WebAPI.Services;
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
            if (parent.Data is null) return;

            parent.Data.IsFailed = false;
            parent.Data.ValidationMessage = null;
            parent.Data.DatasetColumns = null;
        }

        public abstract Task Hydrate(WorkflowContext context, WorkflowNodeDto root);
        public abstract Task Run(WorkflowContext context, WorkflowNodeDto root, CancellationToken cancellationToken);
        public abstract bool Validate(WorkflowContext context, WorkflowNodeDto root);
        public virtual void PropagateDatasetColumns(WorkflowContext context, WorkflowNodeDto root)
        {
            var parentDatasetColumns = root.Parent?.Data?.DatasetColumns;

            root.SetDatasetColumns(parentDatasetColumns);
        }

        virtual public async Task GenerateOuput(WorkflowContext context, WorkflowNodeDto root, CancellationToken cancellationToken)
        {
            if (context.EstimatorChain is null) return;

            var sw = Stopwatch.StartNew();

            var transformer = context.EstimatorChain.Fit(context.DataView);
            var dataview = transformer.Transform(context.DataView);
            using var stream = new MemoryStream();
            context.MLContext.Data.SaveAsBinary(dataview, stream);
            stream.Seek(0, SeekOrigin.Begin);

            var workflowDataView = await _operatorService.AddWorkflowDataView(context.Workflow, root.Id, root.Type, stream, cancellationToken);

            root.Data.Parameters = new Dictionary<string, object>();
            root.Data.Parameters.Add("WorkflowDataViewId", workflowDataView?.Id);

            sw.Stop();
            Console.WriteLine($"Node {root.Data?.Name}, ellapsed: {sw.ElapsedMilliseconds}ms");
        }
    }
}
