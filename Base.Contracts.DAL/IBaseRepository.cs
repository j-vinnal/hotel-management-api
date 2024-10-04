using Base.Contracts;

namespace Base.Contracts.DAL;

public interface IBaseRepository<TEntity> : IBaseRepository<TEntity, Guid>
    where TEntity : class, IEntityId
{
}

public interface IBaseRepository<TEntity, TKey>
    where TEntity : class, IEntityId<TKey>
    where TKey : struct, IEquatable<TKey>
{
    //TODO too expensive
    //Have to think how to solve dropdown ViewData
    // IEnumerable<TEntity?> All();


    //TODO too expensive
    //tracking is expensive


    Task<TEntity?> FindAsync(TKey id, TKey? userId = default, bool noTracking = true);

    IEnumerable<TEntity> GetAll(TKey userId = default, bool noTracking = true);
    Task<IEnumerable<TEntity>> GetAllAsync(TKey userId = default, bool noTracking = true);

    TEntity Add(TEntity entity);

    TEntity Update(TEntity entity);

    int Remove(TEntity entity, TKey? userId = default, bool noTracking = true);

    Task<int?> RemoveAsync(TEntity entity, TKey? userId = default, bool noTracking = true);

    int Remove(TKey id, TKey? userId = default, bool noTracking = true);

    Task<int?> RemoveAsync(TKey id, TKey? userId = default, bool noTracking = true);

    bool Exists(TKey id, TKey? userId = default);
    Task<bool> ExistsAsync(TKey id, TKey? userId = default);
}