using AIaaS.Domain.Enums;
using MediatR;

namespace AIaaS.Application.Features.Workflows.Notifications
{
    public class WorkflowNodeValidateChangeDto {
        public string? NodeId { get; set; }
        public string? NodeType { get; set; }
        public WorkflowRunStatus Status { get; set; }
        public string? StatusDetail { get; set; }
        public static WorkflowNodeValidateChangeDto Create(string? nodeId, string? nodeType)
        {
            return new WorkflowNodeValidateChangeDto()
            {             
                NodeId = nodeId,
                NodeType = nodeType,
                Status = WorkflowRunStatus.Pending,
                StatusDetail = null
            };
        }
        public void CompleteWorkflowRunHistory(WorkflowRunStatus status, string? statusDetail)
        {
            this.Status = status;
            this.StatusDetail = statusDetail;
        }
    }
    public class WorkflowNodeValidateChangeNotification : INotification
    {
        public WorkflowNodeValidateChangeNotification(WorkflowNodeValidateChangeDto workflowNodeValidateChangeDto)
        {
            WorkflowNodeValidateChange = workflowNodeValidateChangeDto;
        }

        public WorkflowNodeValidateChangeDto WorkflowNodeValidateChange { get; }
    }
}
