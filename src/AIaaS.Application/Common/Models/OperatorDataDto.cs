using AIaaS.Domain.Enums;

namespace AIaaS.Application.Common.Models.Dtos
{
    public class OperatorDataDto
    {
        public string Name { get; set; }
        public IList<OperatorConfigurationDto>? Config { get; set; }
        public WorkflowRunStatus? Status { get; set; }
        public string? StatusDetail { get; set; }
        public IDictionary<string, object>? Parameters { get; set; }
        public IList<string>? DatasetColumns { get; set; }
    }
}
