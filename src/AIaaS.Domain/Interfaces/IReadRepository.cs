using Ardalis.Specification;

namespace AIaaS.Domain.Interfaces
{
    public interface IReadRepository<T> : IReadRepositoryBase<T> where T : class, IAggregateRoot
    {
    }
}
