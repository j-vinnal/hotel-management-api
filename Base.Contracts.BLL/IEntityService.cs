
using Base.Contracts.DAL;

namespace Base.Contracts.BLL;

public interface IEntityService<TEntity> : IBaseRepository<TEntity>, IEntityService<TEntity, Guid>
    where TEntity : class, IEntityId
{
}

public interface IEntityService<TEntity, TKey> : IBaseRepository<TEntity, TKey>, IService
    where TEntity : class, IEntityId<TKey>
    where TKey : struct, IEquatable<TKey>
{
}