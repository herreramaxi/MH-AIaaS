namespace AIaaS.WebAPI.Models.Dtos
{
    public class WorkflowNodeDto : OperatorDto
    {
        public string Id { get; set; }
        public IList<WorkflowNodeDto>? Children { get; set; }
        public IList<string> OutputColumns { get; set; }

        public void SetAsFailed(string errorMessage)
        {
            if (this.Data is null) return;

            this.Data.ValidationMessage = errorMessage;
            this.Data.IsFailed = true;
        }

        public void SetAsSuccess()
        {
            if (this.Data is null) return;

            this.Data.ValidationMessage = null;
            this.Data.IsFailed = false;
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
            if (this.Data?.Config is null || !this.Data.Config.Any()) return null;

            return this.Data.Config.FirstOrDefault(x => x.Name.Equals(parameterName, StringComparison.InvariantCultureIgnoreCase));
        }

        public void PropagateDatasetColumns(IList<string>? datasetColumns)
        {
            if (this.Data is null || datasetColumns is null) return;

            this.Data.DatasetColumns = datasetColumns;
        }
    }
}
