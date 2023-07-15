using AIaaS.Application.Common.Models;
using FluentValidation;

namespace AIaaS.Application.Features.Datasets.Commands.CreateDataset
{
    public class CreateDatasetParameterValidator: AbstractValidator<CreateDatasetParameter>
    {
        public CreateDatasetParameterValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.ColumnSettings).NotEmpty();
            RuleFor(x => x.Delimiter).NotEmpty();            
        }
    }
}
