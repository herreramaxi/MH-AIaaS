using AIaaS.Application.Common.Models;
using AIaaS.Domain.Common;
using AutoMapper;

namespace AIaaS.WebAPI.Mappings
{
    public class AuditableEntityProfile: Profile
    {
        public AuditableEntityProfile()
        {
            CreateMap<AuditableEntity, AuditableEntityDto>();
        }
    }
}
