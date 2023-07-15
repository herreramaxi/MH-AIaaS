using AIaaS.Application.Common.Models;
using AIaaS.Domain.Entities;
using AutoMapper;

namespace AIaaS.WebAPI.Mappings
{
    public class DatasetProfile : Profile
    {
        public DatasetProfile()
        {
            CreateMap<Dataset, DatasetDto>()
                .ForMember(x => x.FileName, opt => opt.MapFrom(y => y.FileStorage == null ? null : y.FileStorage.FileName))
                .ForMember(x => x.Size, opt => opt.MapFrom(y => y.FileStorage == null ? null : (long?)y.FileStorage.Size))
                .ForMember(x => x.DataViewFileName, opt => opt.MapFrom(y => y.DataViewFile == null ? null : y.DataViewFile.Name))
                .ForMember(x => x.DataViewFileSize, opt => opt.MapFrom(y => y.DataViewFile == null ? null : (long?)y.DataViewFile.Size));

            CreateMap<CreateDatasetParameter, Dataset>()
                .ForMember(x => x.DataViewFile, opt => opt.Ignore())
                .ForMember(x => x.FileStorage, opt => opt.Ignore());
        }
    }
}
