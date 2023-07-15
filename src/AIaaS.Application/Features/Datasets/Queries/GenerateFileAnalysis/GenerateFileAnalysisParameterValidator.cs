using AIaaS.Application.Features.Datasets.Queries.GenerateFileAnalysis;
using FluentValidation;

namespace AIaaS.Application.Features.Datasets.Queries.GenerateFilePreview
{
    public class GenerateFileAnalysisParameterValidator: AbstractValidator<GenerateFileAnalysisParameter>
    {
        public GenerateFileAnalysisParameterValidator()
        {
            RuleFor(x => x.File).NotNull();
            RuleFor(x => x.Delimiter).NotEmpty();            
        }
    }
}
