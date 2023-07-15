using AIaaS.Application.Common.Models;
using AIaaS.Domain.Entities;
using AutoMapper;

namespace AIaaS.WebAPI.Mappings
{
    public class FileStorageProfile : Profile
    {
        public FileStorageProfile()
        {
            CreateMap<FileStorage, FileStorageDto>();
        }
    }
}
