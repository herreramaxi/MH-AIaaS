using AIaaS.Domain.Entities;
using Ardalis.Specification;

namespace AIaaS.Application.Specifications
{
    public class DatasetByIdWithColumnSettingsAndDataViewFileSpec: SingleResultSpecification<Dataset>
    {
        public DatasetByIdWithColumnSettingsAndDataViewFileSpec(int datasetId)
        {
            Query
                .Where(x => x.Id == datasetId)
                .Include(x => x.ColumnSettings)
                .Include(x => x.DataViewFile);
        }
    }
}
