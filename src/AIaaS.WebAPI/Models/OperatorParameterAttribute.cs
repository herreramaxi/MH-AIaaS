namespace AIaaS.WebAPI.Models
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class OperatorParameterAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }
        public string Type { get; }
        public OperatorParameterAttribute(string name, string description, string type)
        {
            Name = name;
            Description = description;
            Type = type;
        }
    }
}
