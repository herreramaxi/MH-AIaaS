namespace AIaaS.WebAPI.Models.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class OperatorParameterAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }
        public string Type { get; }
        public string? Default { get; }

        public OperatorParameterAttribute(string name, string description, string type, string? defaultValue = null)
        {
            Name = name;
            Description = description;
            Type = type;
            Default = defaultValue;
        }
    }
}
