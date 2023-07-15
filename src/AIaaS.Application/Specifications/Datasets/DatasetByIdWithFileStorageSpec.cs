using AIaaS.Domain.Entities;
using Ardalis.Specification;

namespace AIaaS.Application.Specifications.Datasets
{
    public class DatasetByIdWithFileStorageSpec : SingleResultSpecification<Dataset>
    {
        public DatasetByIdWithFileStorageSpec(int datasetId)
        {
            Query
                .Include(x => x.FileStorage)
                .Where(x => x.Id == datasetId);
        }
    }
}
