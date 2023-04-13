using AIaaS.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AIaaS.WebAPI.Data
{
    public class AIaaSContext : DbContext
    {
        public AIaaSContext(DbContextOptions<AIaaSContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        //protected override void OnConfiguring(DbContextOptionsBuilder options)
        //=> options.UseSqlServer($"Data Source={DbPath}");
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);
        //}
    }
}
