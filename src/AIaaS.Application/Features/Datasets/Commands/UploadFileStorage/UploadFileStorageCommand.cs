using AIaaS.Application.Common.Models;
using AIaaS.Domain.Entities;
using Ardalis.Result;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Commands.UploadDataset
{
    public class UploadFileStorageCommand : IRequest<Result>
    {
        public UploadFileStorageCommand(UploadFileStorageParameter uploadFileStorageParameter)
        {
            UploadFileStorageParameter = uploadFileStorageParameter;
        }

        public UploadFileStorageParameter UploadFileStorageParameter { get; }
    }
}
