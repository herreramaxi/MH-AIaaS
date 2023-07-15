using AIaaS.Domain.Entities;
using Ardalis.Specification;

namespace AIaaS.Application.Specifications.Datasets
{
    public class DatasetByIdWithColumnSettingsSpec : SingleResultSpecification<Dataset>
    {
        public DatasetByIdWithColumnSettingsSpec(int datasetId)
        {
            Query
                .Include(x => x.ColumnSettings)
                .Where(x => x.Id == datasetId);        
        }
    }
}
