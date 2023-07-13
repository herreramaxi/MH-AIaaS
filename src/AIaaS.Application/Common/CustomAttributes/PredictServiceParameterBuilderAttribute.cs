using AIaaS.Domain.Entities.enums;

namespace AIaaS.Application.Common.Models.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PredictServiceParameterBuilderAttribute : Attribute
    {
        public PredictServiceBuilderType BuilderType { get; }
        public int Order { get; }     

        public PredictServiceParameterBuilderAttribute(PredictServiceBuilderType builderType, int order)
        {
            BuilderType = builderType;
            Order = order;
        }
    }
}
