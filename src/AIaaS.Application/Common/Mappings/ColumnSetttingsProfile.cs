using AIaaS.Application.Common.Models;
using AIaaS.Domain.Entities;
using AutoMapper;

namespace AIaaS.WebAPI.Mappings
{
    public class ColumnSetttingsProfile : Profile
    {
        public ColumnSetttingsProfile()
        {
            CreateMap<ColumnSetting, ColumnSettingDto>();
            CreateMap<ColumnSettingDto,ColumnSetting>();
        }
    }
}
