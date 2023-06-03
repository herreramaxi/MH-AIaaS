﻿using AIaaS.WebAPI.Models.Dtos;

namespace AIaaS.WebAPI.ExtensionMethods
{
    public static class WorkflowNodeDtoExtensions
    {
        public static U? GetParameterValue<T, U>(this WorkflowNodeDto node, string parameterName, Func<T, U?> conversionFn) where T : IConvertible
        {            
            var op = node.GetOperatorConfig(parameterName);
            var valueString = op?.Value?.ToString();
            if (string.IsNullOrEmpty(valueString)) return default(U?);

            var value = (T)Convert.ChangeType(valueString, typeof(T));
         
            var converted = conversionFn(value);
            return converted;
        }

        public static T? GetParameterValue<T>(this WorkflowNodeDto node, string parameterName) where T : struct, IConvertible
        {
            var op = node.GetOperatorConfig(parameterName);
            var value = op?.Value?.ToString();
            if (string.IsNullOrEmpty(value)) return null;
            return (T)Convert.ChangeType(value, typeof(T));
        }


        public static OperatorConfigurationDto? GetOperatorConfig(this WorkflowNodeDto node, string parameterName)
        {
            if (node.Data?.Config is null || !node.Data.Config.Any()) return null;

            return node.Data.Config.FirstOrDefault(x => x.Name.Equals(parameterName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
