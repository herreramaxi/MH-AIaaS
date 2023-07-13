using AIaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    public DbSet<Dataset> Datasets { get; set; }
    public DbSet<ColumnSetting> ColumnSettings { get; set; }
    public DbSet<FileStorage> FileStorages { get; set; }
    public DbSet<Workflow> Workflows { get; set; }
    public DbSet<WorkflowDataView> WorkflowDataViews { get; set; }
    public DbSet<DataViewFile> DataViewFiles { get; set; }
    public DbSet<MLModel> MLModels { get; set; }
    public DbSet<ModelMetrics> ModelMetrics { get; set; }
    public DbSet<MLEndpoint> Endpoints { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
