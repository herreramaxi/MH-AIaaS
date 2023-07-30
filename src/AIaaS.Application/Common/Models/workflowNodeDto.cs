using System.Text.Json.Serialization;

namespace AIaaS.Application.Common.Models.Dtos
{
    public class WorkflowNodeDto : OperatorDto
    {
        public string Id { get; set; }
        public IList<WorkflowNodeDto>? Children { get; set; }
        public IList<string> OutputColumns { get; set; }

        [JsonIgnore]
        public WorkflowNodeDto? Parent { get;  set; }
        public void SetAsFailed(string errorMessage)
        {           
            this.Data.StatusDetail = errorMessage;
            this.Data.Status = Domain.Enums.WorkflowRunStatus.Failed;
        }

        public void SetAsSuccess()
        {
            this.Data.StatusDetail = null;
            this.Data.Status = Domain.Enums.WorkflowRunStatus.Finished;
        }

        public void SetAsRunning()
        {
            this.Data.StatusDetail = null;
            this.Data.Status = Domain.Enums.WorkflowRunStatus.Running;
        }

        public U? GetParameterValue<T, U>(string parameterName, Func<T, U?> conversionFn) where T : IConvertible
        {
            var op = this.GetOperatorConfig(parameterName);
            var valueString = op?.Value?.ToString();
            if (string.IsNullOrEmpty(valueString)) return default(U?);

            var value = (T)Convert.ChangeType(valueString, typeof(T));

            var converted = conversionFn(value);
            return converted;
        }

        public T? GetParameterValue<T>(string parameterName) where T : struct, IConvertible
        {
            var op = this.GetOperatorConfig(parameterName);
            var value = op?.Value?.ToString();
            if (string.IsNullOrEmpty(value)) return null;
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public string? GetParameterValue(string parameterName)
        {
            var op = this.GetOperatorConfig(parameterName);
            return op?.Value?.ToString();
        }

        public OperatorConfigurationDto? GetOperatorConfig(string parameterName)
        {
            if (this.Data.Config is null || !this.Data.Config.Any()) return null;

            return this.Data.Config.FirstOrDefault(x => x.Name.Equals(parameterName, StringComparison.InvariantCultureIgnoreCase));
        }

        public void SetDatasetColumns(IList<string>? datasetColumns)
        {
            this.Data.DatasetColumns = datasetColumns;
        }       
    }
}
