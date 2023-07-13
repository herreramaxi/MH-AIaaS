using AIaaS.Domain.Entities.enums;

namespace AIaaS.Application.Common.Models.CustomAttributes
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
