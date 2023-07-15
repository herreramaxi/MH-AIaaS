using AIaaS.Domain.Interfaces;
using Ardalis.Specification.EntityFrameworkCore;

namespace AIaaS.Infrastructure.Data
{
    public class EfRepository<T> : RepositoryBase<T>, IReadRepository<T>, IRepository<T> where T : class, IAggregateRoot
    {
        public EfRepository(EfContext dbContext) : base(dbContext)
        {
        }
    }
}