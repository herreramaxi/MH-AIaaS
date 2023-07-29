using AIaaS.Domain.Entities;
using Ardalis.Specification;

namespace AIaaS.Application.Specifications.Datasets
{
    public class DatasetGetAllSpec : Specification<Dataset>
    {
        public DatasetGetAllSpec()
        {
            Query
                .Include(x => x.FileStorage)
                .OrderBy(x => x.Name);
        }
    }
}
