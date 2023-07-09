using AIaaS.WebAPI.Models;
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
