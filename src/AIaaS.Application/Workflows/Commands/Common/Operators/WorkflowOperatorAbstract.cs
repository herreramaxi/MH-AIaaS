using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Domain.Entities;
using AIaaS.WebAPI.Interfaces;
using CleanArchitecture.Application.Common.Interfaces;
using Microsoft.ML;

namespace AIaaS.Application.Common.Models.Operators
{
    public abstract class WorkflowOperatorAbstract : IWorkflowOperator
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public WorkflowOperatorAbstract()
        {
            var operatorAttribute = (OperatorAttribute?)this.GetType().GetCustomAttributes(typeof(OperatorAttribute), false).FirstOrDefault();
            if (operatorAttribute is null) throw new Exception($"OperatorAttribute not found on type {this.GetType().FullName}");

            Name = operatorAttribute.Name;
            Type = operatorAttribute.Type;
        }

        public virtual void Preprocessing(WorkflowContext context, WorkflowNodeDto parent)
        {
            if (parent.Data is null) return;

            parent.Data.IsFailed = false;
            parent.Data.ValidationMessage = null;
            parent.Data.DatasetColumns = null;
        }

        public abstract Task Hydrate(WorkflowContext context, WorkflowNodeDto root);
        public abstract Task Run(WorkflowContext context, Dtos.WorkflowNodeDto root);
        public abstract bool Validate(WorkflowContext context, WorkflowNodeDto root);
        public virtual void PropagateDatasetColumns(WorkflowContext context, WorkflowNodeDto root)
        {
            var parentDatasetColumns = root.Parent?.Data?.DatasetColumns;

            root.SetDatasetColumns(parentDatasetColumns);
        }

        public void Postprocessing(WorkflowContext context, WorkflowNodeDto root)
        {
            root.Parent = null;
        }
        virtual public async Task GenerateOuput(WorkflowContext context, WorkflowNodeDto root, IApplicationDbContext dbContext)
        {
            if (context.EstimatorChain is null) return;

            var transformer = context.EstimatorChain.Fit(context.DataView);
            var dataview = transformer.Transform(context.DataView);
            using var stream = new MemoryStream();
            context.MLContext.Data.SaveAsBinary(dataview, stream);

            var dataView = context.Workflow.WorkflowDataViews.FirstOrDefault(x => x.NodeId.Equals(root.Id, StringComparison.InvariantCultureIgnoreCase));

            if (dataView is null)
            {
                dataView = new WorkflowDataView
                {
                    Size = stream.Length,
                    Data = stream.ToArray(),
                    NodeId = root.Id,
                    NodeType = root.Type
                };

                context.Workflow.WorkflowDataViews.Add(dataView);
            }
            else
            {
                dataView.Size = stream.Length;
                dataView.Data = stream.ToArray();
            }

            await dbContext.SaveChangesAsync();

            root.Data.Parameters = new Dictionary<string, object>();
            root.Data.Parameters.Add("WorkflowDataViewId", dataView.Id);
        }
    }
}
