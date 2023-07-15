using AIaaS.Domain.Entities;
using Ardalis.Specification;

namespace AIaaS.Application.Specifications.Datasets
{
    public class DatasetByIdWithDataViewFileSpec : SingleResultSpecification<Dataset>
    {
        public DatasetByIdWithDataViewFileSpec(int datasetId)
        {
            Query
                .Include(x => x.DataViewFile)
                .Where(x => x.Id == datasetId);        
        }
    }
}
