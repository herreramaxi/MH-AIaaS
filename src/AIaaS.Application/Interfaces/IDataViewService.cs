using AIaaS.Application.Common.Models;
using AIaaS.Domain.Entities;
using Ardalis.Result;
using Microsoft.ML;

namespace AIaaS.Application.Interfaces
{
    public interface IDataViewService
    {
        Task<Result<DataViewFilePreviewDto>> GetPreviewAsync(DataViewFile dataViewFile);
        Task<Result<IDataView>> GetDataViewAsync(DataViewFile dataViewFile);
    }
}
