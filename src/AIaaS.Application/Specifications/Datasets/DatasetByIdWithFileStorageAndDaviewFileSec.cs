using AIaaS.Domain.Entities;
using Ardalis.Specification;

namespace AIaaS.Application.Specifications.Datasets
{
    public class DatasetByIdWithFileStorageAndDaviewFileSec : SingleResultSpecification<Dataset, Dataset>
    {
        public DatasetByIdWithFileStorageAndDaviewFileSec(int datasetId)
        {
            Query
                .Include(x => x.FileStorage)
                .Include(x => x.DataViewFile)
                .Where(x => x.Id == datasetId);

            Query.Select(x => new Dataset
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                CreatedBy = x.CreatedBy,
                ModifiedBy = x.ModifiedBy,
                CreatedOn = x.CreatedOn,
                ModifiedOn = x.ModifiedOn,
                FileStorage = new FileStorage
                {
                    Size = x.FileStorage.Size,
                    FileName = x.FileStorage.FileName
                },
                DataViewFile = new DataViewFile
                {
                    Size = x.DataViewFile.Size,
                    Name = x.DataViewFile.Name
                }
            });
        }
    }
}
