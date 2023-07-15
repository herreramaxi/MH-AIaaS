using AIaaS.Domain.Entities;
using Ardalis.Specification;

namespace AIaaS.Application.Specifications.Datasets
{
    public class DatasetGetAllSpec : Specification<Dataset>
    {
        public DatasetGetAllSpec()
        {
            Query.
             OrderBy(x => x.Name);
        }
    }
}
