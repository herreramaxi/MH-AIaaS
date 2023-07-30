using AIaaS.Application.Common.Models;
using AIaaS.Domain.Interfaces;
using Ardalis.Result;
using Microsoft.ML;

namespace AIaaS.Application.Interfaces
{
    public interface IDataViewService
    {
        Task<Result<DataViewFilePreviewDto>> GetPreviewAsync(IDataViewFile dataViewFile);
        Task<Result<IDataView>> GetDataViewAsync(IDataViewFile dataViewFile);
    }
}
