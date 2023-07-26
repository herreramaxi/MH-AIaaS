using AIaaS.Domain.Entities;
using Ardalis.Specification;

namespace AIaaS.Application.Specifications.Datasets
{
    public class DatasetByIdWithFileStorageAndDaviewFileSec : SingleResultSpecification<Dataset>
    {
        public DatasetByIdWithFileStorageAndDaviewFileSec(int datasetId)
        {
            Query
                .Include(x => x.FileStorage)
                .Include(x => x.DataViewFile)
                .Where(x => x.Id == datasetId);
        }
    }
}
