using Base.Contracts;
using Base.Contracts.DAL;
using Base.Contracts.Domain;
using Microsoft.EntityFrameworkCore;

namespace Base.DAL.EF;

public class
    BaseEntityRepository<TDomainEntity, TDalEntity, TDbContext> :
    BaseEntityRepository<Guid, TDomainEntity, TDalEntity, TDbContext>, IBaseEntityRepository<TDalEntity>
    where TDomainEntity : class, IEntityId
    where TDalEntity : class, IEntityId
    where TDbContext : DbContext
{
    public BaseEntityRepository(TDbContext dataContext, IEntityMapper<TDomainEntity, TDalEntity> entityMapper) : base(dataContext,
        entityMapper)
    {
    }
}

public class BaseEntityRepository<TKey, TDomainEntity, TDalEntity, TDbContext> : IBaseEntityRepository<TDalEntity, TKey>
    where TDomainEntity : class, IEntityId<TKey>
    where TDalEntity : class, IEntityId<TKey>
    where TKey : struct, IEquatable<TKey>
    where TDbContext : DbContext
{
    protected readonly IEntityMapper<TDomainEntity, TDalEntity> EntityMapper;
    protected TDbContext RepositoryDbContext;
    protected DbSet<TDomainEntity> RepositoryDbSet;

    public BaseEntityRepository(TDbContext dataContext, IEntityMapper<TDomainEntity, TDalEntity> entityMapper)
    {
        RepositoryDbContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        EntityMapper = entityMapper;
        RepositoryDbSet = RepositoryDbContext.Set<TDomainEntity>();
    }

    
    public virtual async Task<TDalEntity?> FindAsync(TKey id, TKey? userId = default, bool noTracking = true)
    {
        var test =EntityMapper.Map(await CreateQuery(userId, noTracking)
            .FirstOrDefaultAsync(e => e.Id.Equals(id)));
        return test;
    }


    public virtual IEnumerable<TDalEntity> GetAll(TKey userId = default, bool noTracking = true)
    {
        return CreateQuery(userId, noTracking).ToList().Select(e => EntityMapper.Map(e))!;
    }

    public virtual async Task<IEnumerable<TDalEntity>> GetAllAsync(TKey userId = default, bool noTracking = true)
    {
        return (await CreateQuery(userId, noTracking).ToListAsync()).Select(e => EntityMapper.Map(e))!;
    }


    public virtual TDalEntity Add(TDalEntity entity)
    {
        return EntityMapper.Map(RepositoryDbSet.Add(EntityMapper.Map(entity)!).Entity)!;
    }

    public virtual TDalEntity Update(TDalEntity entity)
    {
        return EntityMapper.Map(RepositoryDbSet.Update(EntityMapper.Map(entity)!).Entity)!;
    }


    public virtual int Remove(TDalEntity entity, TKey? userId = default, bool noTracking = true)
    {
        if (userId != null && !userId.Equals(default(TKey)))
            return CreateQuery(userId, noTracking)
                .Where(e => e.Id.Equals(entity.Id))
                .ExecuteDelete();


        return RepositoryDbSet
            .Where(e => e.Id.Equals(entity.Id))
            .ExecuteDelete();
    }

    public virtual async Task<int?> RemoveAsync(TDalEntity entity, TKey? userId = default, bool noTracking = true)
    {
        if (userId != null && !userId.Equals(default(TKey)))
            return await CreateQuery(userId, noTracking)
                .Where(e => e.Id.Equals(entity.Id))
                .ExecuteDeleteAsync();

        return await RepositoryDbSet
            .Where(e => e.Id.Equals(entity.Id))
            .ExecuteDeleteAsync();
    }


    public virtual int Remove(TKey id, TKey? userId = default, bool noTracking = true)
    {
        if (userId != null && !userId.Equals(default(TKey)))
            return CreateQuery(userId, noTracking)
                .Where(e => e.Id.Equals(id))
                .ExecuteDelete();

        return RepositoryDbSet
            .Where(e => e.Id.Equals(id))
            .ExecuteDelete();
    }

    public virtual async Task<int?> RemoveAsync(TKey id, TKey? userId = default, bool noTracking = true)
    {
        if (userId != null && !userId.Equals(default(TKey)))
            return await CreateQuery(userId, noTracking)
                .Where(e => e.Id.Equals(id))
                .ExecuteDeleteAsync();

        return await RepositoryDbSet
            .Where(e => e.Id.Equals(id))
            .ExecuteDeleteAsync();
    }


    public virtual bool Exists(TKey id, TKey? userId = default)
    {
        if (userId != null && !userId.Equals(default(TKey)))
            return CreateQuery(userId)
                .Any(e => e.Id.Equals(id));

        return RepositoryDbSet
            .Any(e => e.Id.Equals(id));
    }

    public virtual async Task<bool> ExistsAsync(TKey id, TKey? userId = default)
    {
        if (userId != null && !userId.Equals(default(TKey)))
            return await CreateQuery(userId)
                .AnyAsync(e => e.Id.Equals(id));

        return await RepositoryDbSet
            .AnyAsync(e => e.Id.Equals(id));
    }

    protected IQueryable<TDomainEntity> CreateQuery(TKey? userId = default, bool noTracking = true)
    {
        var query = RepositoryDbSet.AsQueryable();

        if (userId != null && !userId.Equals(default(TKey)) &&
            typeof(IDomainAppUserId<TKey>).IsAssignableFrom(typeof(TDomainEntity)))
            query = query
                .Include("User")
                .Where(e => ((IDomainAppUserId<TKey>)e).AppUserId.Equals(userId));

        if (noTracking) query = query.AsNoTracking();

        return query;
    }
}