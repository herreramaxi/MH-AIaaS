using AIaaS.Application.Common.Models;
using FluentValidation;

namespace AIaaS.Application.Features.Datasets.Commands.CreateDataset
{
    public class UploadFileStorageParameterValidator : AbstractValidator<UploadFileStorageParameter>
    {
        public UploadFileStorageParameterValidator()
        {
            RuleFor(x => x.DatasetId).GreaterThan(0);
            RuleFor(x => x.File).NotNull();
        }
    }
}
