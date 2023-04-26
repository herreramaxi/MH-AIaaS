using AIaaS.WebAPI.ExtensionMethods;
using AIaaS.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AIaaS.WebAPI.Data
{
    public class EfContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EfContext(DbContextOptions<EfContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Dataset> Datasets { get; set; }
        public DbSet<ColumnSetting> ColumnSettings { get; set; }
        public DbSet<FileStorage> FileStorages { get; set; }
        public DbSet<Workflow> Workflows { get; set; }
        //protected override void OnConfiguring(DbContextOptionsBuilder options)
        //=> options.UseSqlServer($"Data Source={DbPath}");
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);
        //}
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            this.SetAuditProperties();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            this.SetAuditProperties();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        private void SetAuditProperties(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Get all the entities that inherit from AuditableEntity
            // and have a state of Added or Modified
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is AuditableEntity && (
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified));

            // For each entity we will set the Audit properties
            foreach (var entityEntry in entries)
            {
                // If the entity state is Added let's set
                // the CreatedAt and CreatedBy properties
                if (entityEntry.State == EntityState.Added)
                {
                    ((AuditableEntity)entityEntry.Entity).CreatedOn = DateTime.UtcNow;
                    ((AuditableEntity)entityEntry.Entity).CreatedBy = _httpContextAccessor?.HttpContext?.User?.GetEmail() ?? "N/A";
                }
                else
                {
                    // If the state is Modified then we don't want
                    // to modify the CreatedAt and CreatedBy properties
                    // so we set their state as IsModified to false
                    Entry((AuditableEntity)entityEntry.Entity).Property(p => p.CreatedOn).IsModified = false;
                    Entry((AuditableEntity)entityEntry.Entity).Property(p => p.CreatedBy).IsModified = false;
                }

                // In any case we always want to set the properties
                // ModifiedAt and ModifiedBy
                ((AuditableEntity)entityEntry.Entity).ModifiedOn = DateTime.UtcNow;
                ((AuditableEntity)entityEntry.Entity).ModifiedBy = _httpContextAccessor?.HttpContext?.User?.GetEmail() ?? "N/A";
            }

            // After we set all the needed properties
            // we call the base implementation of SaveChangesAsync
            // to actually save our entities in the database
            //return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
