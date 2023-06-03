using AIaaS.WebAPI.Models.Dtos;
using AIaaS.WebAPI.Models.enums;
using AIaaS.WebAPI.Models.Operators;

namespace AIaaS.WebAPI.Models
{
    [Operator("Normalize", OperatorType.Normalize, 7)]
    [OperatorParameter("Variable Name", "The name of the new or existing variable", "text")]
    [OperatorParameter("Value", "Javascript expression for the value", "text")]
    public class NormalizeOperator : WorkflowOperatorAbstract
    {
        public override Task Execute(WorkflowContext mlContext, WorkflowNodeDto root)
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }
    }
}
