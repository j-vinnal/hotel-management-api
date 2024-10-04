using Base.Contracts;
using Base.Contracts.BLL;
using Base.Contracts.DAL;

namespace Base.BLL;

public class
    BaseEntityService<TBllEntity, TDalEntity, TRepository> :
    BaseEntityService<TBllEntity, TDalEntity, TRepository, Guid>, IEntityService<TBllEntity>
    where TBllEntity : class, IEntityId
    where TDalEntity : class, IEntityId
    where TRepository : IBaseRepository<TDalEntity>

{
    public BaseEntityService(TRepository repository, IMapper<TDalEntity, TBllEntity> mapper) : base(repository, mapper)
    {
    }
}

public class BaseEntityService<TBllEntity, TDalEntity, TRepository, TKey> : IEntityService<TBllEntity, TKey>
    where TBllEntity : class, IEntityId<TKey>
    where TDalEntity : class, IEntityId<TKey>
    where TRepository : IBaseRepository<TDalEntity, TKey>
    where TKey : struct, IEquatable<TKey>

{
    protected readonly IMapper<TDalEntity, TBllEntity> Mapper;
    protected readonly TRepository Repository;

    public BaseEntityService(TRepository repository, IMapper<TDalEntity, TBllEntity> mapper)
    {
        Repository = repository;
        Mapper = mapper;
    }


    public virtual async Task<TBllEntity?> FindAsync(TKey id, TKey? userId = default, bool noTracking = true)
    {
        return Mapper.Map(await Repository.FindAsync(id, userId, noTracking));
    }

    public virtual IEnumerable<TBllEntity> GetAll(TKey userId = default, bool noTracking = true)
    {
        return Repository.GetAll(userId, noTracking).Select(e => Mapper.Map(e))!;
    }


    public virtual async Task<IEnumerable<TBllEntity>> GetAllAsync(TKey userId = default, bool noTracking = true)
    {
        return (await Repository.GetAllAsync(userId, noTracking)).Select(e => Mapper.Map(e))!;
    }

    public virtual TBllEntity Add(TBllEntity entity)
    {
        return Mapper.Map(Repository.Add(Mapper.Map(entity)!))!;
    }

    public virtual TBllEntity Update(TBllEntity entity)
    {
        return Mapper.Map(Repository.Update(Mapper.Map(entity)!))!;
    }

    public virtual int Remove(TBllEntity entity, TKey? userId = default, bool noTracking = true)
    {
        return Repository.Remove(Mapper.Map(entity)!, userId);
    }

    public virtual async Task<int?> RemoveAsync(TBllEntity entity, TKey? userId = default, bool noTracking = true)
    {
        return await Repository.RemoveAsync(Mapper.Map(entity)!, userId);
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