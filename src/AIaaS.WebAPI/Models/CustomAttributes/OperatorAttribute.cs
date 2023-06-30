using AIaaS.WebAPI.Models.enums;

namespace AIaaS.WebAPI.Models.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OperatorAttribute : Attribute
    {
        public string Name { get; }
        public string Type { get; }
        public int Order { get; }
        public OperatorAttribute(string name, OperatorType type, int order)
        {
            Name = name;
            Type = type.ToString();
            Order = order;
        }
    }
}
