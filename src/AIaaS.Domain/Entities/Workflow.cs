using AIaaS.Domain.Common;
using AIaaS.Domain.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

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
        [NotMapped]
        public WorkflowRunHistory? CurrentWorkflowRunHistory { get; private set; }

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
          
        public void UpdateData(string? data)
        {
            this.Data = data;
        }      
    }
}