using Base.Contracts;
using Base.Contracts.BLL;
using Base.Contracts.DAL;

namespace Base.BLL;

public class
    BaseEntityService<TBllEntity, TDalEntity, TRepository> :
    BaseEntityService<TBllEntity, TDalEntity, TRepository, Guid>, IEntityService<TBllEntity>
    where TBllEntity : class, IEntityId
    where TDalEntity : class, IEntityId
    where TRepository : IBaseEntityRepository<TDalEntity>

{
    public BaseEntityService(TRepository repository, IEntityMapper<TDalEntity, TBllEntity> entityMapper) : base(repository, entityMapper)
    {
    }
}

public class BaseEntityService<TBllEntity, TDalEntity, TRepository, TKey> : IEntityService<TBllEntity, TKey>
    where TBllEntity : class, IEntityId<TKey>
    where TDalEntity : class, IEntityId<TKey>
    where TRepository : IBaseEntityRepository<TDalEntity, TKey>
    where TKey : struct, IEquatable<TKey>

{
    protected readonly IEntityMapper<TDalEntity, TBllEntity> EntityMapper;
    protected readonly TRepository Repository;

    public BaseEntityService(TRepository repository, IEntityMapper<TDalEntity, TBllEntity> entityMapper)
    {
        Repository = repository;
        EntityMapper = entityMapper;
    }


    public virtual async Task<TBllEntity?> FindAsync(TKey id, TKey? userId = default, bool noTracking = true)
    {
        return EntityMapper.Map(await Repository.FindAsync(id, userId, noTracking));
    }

    public virtual IEnumerable<TBllEntity> GetAll(TKey userId = default, bool noTracking = true)
    {
        return Repository.GetAll(userId, noTracking).Select(e => EntityMapper.Map(e))!;
    }


    public virtual async Task<IEnumerable<TBllEntity>> GetAllAsync(TKey userId = default, bool noTracking = true)
    {
        return (await Repository.GetAllAsync(userId, noTracking)).Select(e => EntityMapper.Map(e))!;
    }

    public virtual TBllEntity Add(TBllEntity entity)
    {
        return EntityMapper.Map(Repository.Add(EntityMapper.Map(entity)!))!;
    }

    public virtual TBllEntity Update(TBllEntity entity)
    {
        return EntityMapper.Map(Repository.Update(EntityMapper.Map(entity)!))!;
    }

    public virtual int Remove(TBllEntity entity, TKey? userId = default, bool noTracking = true)
    {
        return Repository.Remove(EntityMapper.Map(entity)!, userId);
    }

    public virtual async Task<int?> RemoveAsync(TBllEntity entity, TKey? userId = default, bool noTracking = true)
    {
        return await Repository.RemoveAsync(EntityMapper.Map(entity)!, userId);
    }

    public virtual int Remove(TKey id, TKey? userId = default, bool noTracking = true)
    {
        return Repository.Remove(id, userId);
    }

    public virtual async Task<int?> RemoveAsync(TKey id, TKey? userId = default, bool noTracking = true)
    {
        return await Repository.RemoveAsync(id, userId);
    }

    public virtual bool Exists(TKey id, TKey? userId = default)
    {
        return Repository.Exists(id, userId);
    }

    public virtual async Task<bool> ExistsAsync(TKey id, TKey? userId = default)
    {
        return await Repository.ExistsAsync(id, userId);
    }
}