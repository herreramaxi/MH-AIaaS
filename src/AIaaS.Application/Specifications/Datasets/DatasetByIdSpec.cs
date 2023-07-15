using AIaaS.Domain.Entities;
using Ardalis.Specification;

namespace AIaaS.Application.Specifications.Datasets
{
    public class DatasetByIdSec : SingleResultSpecification<Dataset>
    {
        public DatasetByIdSec(int datasetId)
        {
            Query
                .Where(x => x.Id == datasetId);
        }
    }
}
