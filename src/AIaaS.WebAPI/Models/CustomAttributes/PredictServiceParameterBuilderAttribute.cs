using AIaaS.WebAPI.Models.enums;

namespace AIaaS.WebAPI.Models.CustomAttributes
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
