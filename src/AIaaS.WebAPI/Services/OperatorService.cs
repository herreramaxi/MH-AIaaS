using AIaaS.WebAPI.Interfaces;
using AIaaS.WebAPI.Models;
using AIaaS.WebAPI.Models.Dtos;

namespace AIaaS.WebAPI.Services
{
    public interface IOperatorService
    {
        IList<OperatorDto> GetOperators();
    }
    public class OperatorService : IOperatorService
    {
        public IList<OperatorDto> GetOperators()
        {
            var operators = new List<OperatorDto>();
            var workflowOperators = typeof(IWebApiMarker)
                .Assembly
                .GetTypes()
                .Where(x => typeof(IWorkflowOperator).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface);

            foreach (var workflowOperator in workflowOperators)
            {
                var operatorAttribute = (OperatorAttribute?)workflowOperator.GetCustomAttributes(typeof(OperatorAttribute), false).FirstOrDefault();
                if (operatorAttribute is null) continue;

                var name = operatorAttribute.Name;
                var order = operatorAttribute.Order;
                var type = operatorAttribute.Type;

                var operatorDto = new OperatorDto()
                {
                    Name = name,
                    Type = type,
                    Order = order,
                    Data = new OperatorDataDto()
                    {
                        Name = name
                    }
                };

                operators.Add(operatorDto);

                var operatorParameters = (OperatorParameterAttribute[]?)workflowOperator.GetCustomAttributes(typeof(OperatorParameterAttribute), false);
                if (operatorParameters is null) continue;

                operatorDto.Data.Config = operatorParameters.Select(x => new OperatorConfigurationDto
                {
                    Name = x.Name,
                    Description = x.Description,
                    Type = x.Type
                }).ToList();
            }

            return operators.OrderBy(x => x.Name).ToList();
        }
    }
}
