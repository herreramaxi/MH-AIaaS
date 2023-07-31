using AIaaS.Domain.Enums;
using MediatR;

namespace AIaaS.Application.Features.Workflows.Notifications
{
    public class WorkflowNodeRunHistoryChangeNotification : INotification
    {
        public WorkflowNodeRunHistoryChangeNotification(
            Guid nodeGuid,
            string nodeType,
            WorkflowRunStatus status,
            string? statusDetail,
            IList<string>? datasetColumns = null,
            IDictionary<string, object>? nodeParameters = null)
        {
            NodeGuid = nodeGuid;
            NodeType = nodeType;
            Status = status;
            StatusDetail = statusDetail;
            DatasetColumns = datasetColumns;
            NodeParameters = nodeParameters;
        }

        public Guid NodeGuid { get; }
        public string NodeType { get; }
        public WorkflowRunStatus Status { get; }
        public string? StatusDetail { get; }
        public IList<string>? DatasetColumns { get; }
        public IDictionary<string, object>? NodeParameters { get; }
    }
}
