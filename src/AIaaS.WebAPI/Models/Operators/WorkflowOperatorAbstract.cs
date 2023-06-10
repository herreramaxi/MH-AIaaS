using AIaaS.WebAPI.Interfaces;
using AIaaS.WebAPI.Models.Dtos;

namespace AIaaS.WebAPI.Models.Operators
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

        public virtual void Preprocessing(WorkflowContext context, WorkflowNodeDto root)
        {
            if (root.Data is null) return;

            root.Data.IsFailed = false;
            root.Data.ValidationMessage = null;
        }

        public abstract Task Hydrate(WorkflowContext context, WorkflowNodeDto root);
        public abstract Task Run(WorkflowContext context, Dtos.WorkflowNodeDto root);
        public abstract bool Validate(WorkflowContext context, WorkflowNodeDto root);
        public virtual void PropagateDatasetColumns(WorkflowContext context, WorkflowNodeDto root, WorkflowNodeDto? child)
        {
            child?.PropagateDatasetColumns(root.Data?.DatasetColumns);
        }
    }
}
