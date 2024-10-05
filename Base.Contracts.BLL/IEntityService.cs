
using Base.Contracts.DAL;

namespace Base.Contracts.BLL;

public interface IEntityService<TEntity> : IBaseEntityRepository<TEntity>, IEntityService<TEntity, Guid>
    where TEntity : class, IEntityId
{
}

public interface IEntityService<TEntity, TKey> : IBaseEntityRepository<TEntity, TKey>, IService
    where TEntity : class, IEntityId<TKey>
    where TKey : struct, IEquatable<TKey>
{
}