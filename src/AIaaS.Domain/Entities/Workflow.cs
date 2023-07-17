using AIaaS.Domain.Common;
using AIaaS.Domain.Enums;
using AIaaS.Domain.Interfaces;

namespace AIaaS.Domain.Entities
{
    public class Workflow : AuditableEntity, IAggregateRoot
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public MLModel? MLModel { get; private set; }
        public string? Data { get; private set; }

        private readonly List<WorkflowDataView> _workflowDataViews = new List<WorkflowDataView>();
        public IReadOnlyCollection<WorkflowDataView> WorkflowDataViews => _workflowDataViews.AsReadOnly();

        private readonly List<WorkflowRunHistory> _workflowRunHistories = new List<WorkflowRunHistory>();
        public IReadOnlyCollection<WorkflowRunHistory> WorkflowRunHistories => _workflowRunHistories.AsReadOnly();

        public void AddOrUpdateMLModelData(MemoryStream stream)
        {
            if (MLModel == null)
            {
                MLModel = new MLModel
                {
                    Workflow = this
                };

                MLModel.SetData(stream);
            }
        }

        public WorkflowDataView AddOrUpdateDataView(string nodeId, string nodeType, MemoryStream dataViewStream)
        {
            var dataView = this.WorkflowDataViews.FirstOrDefault(x => x.NodeId.Equals(nodeId, StringComparison.InvariantCultureIgnoreCase));

            if (dataView is null)
            {
                dataView = new WorkflowDataView
                {
                    Workflow = this,
                    NodeId = nodeId,
                    NodeType = nodeType,
                    Size = dataViewStream.Length,
                    Data = dataViewStream.ToArray()
                };

                _workflowDataViews.Add(dataView);
            }
            else
            {
                dataView.Size = dataViewStream.Length;
                dataView.Data = dataViewStream.ToArray();
            }

            return dataView;
        }

        public void UpdateData(string data)
        {
            this.Data = data;
        }

        public WorkflowRunHistory AddWorkflowRunHistory(WorkflowRunStatus status,  DateTime startDate, DateTime? endDate = null)
        {
            var workflowRunHistory = new WorkflowRunHistory()
            {
                Status = status,
                StartDate = startDate,
                EndDate = endDate
            };

            _workflowRunHistories.Add(workflowRunHistory);

            return workflowRunHistory;
        }
    }
}