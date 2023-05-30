using AIaaS.WebAPI.Interfaces;
using Microsoft.ML;

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
        public abstract Task Execute(WorkflowContext mlContext, Dtos.WorkflowNodeDto root);
    }
}
