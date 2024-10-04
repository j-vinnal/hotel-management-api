using Base.Contracts;
using Base.Contracts.DAL;
using Base.Contracts.Domain;
using Microsoft.EntityFrameworkCore;

namespace Base.DAL.EF;

public class
    BaseEntityRepository<TDomainEntity, TDalEntity, TDbContext> :
    BaseEntityRepository<Guid, TDomainEntity, TDalEntity, TDbContext>, IBaseRepository<TDalEntity>
    where TDomainEntity : class, IEntityId
    where TDalEntity : class, IEntityId
    where TDbContext : DbContext
{
    public BaseEntityRepository(TDbContext dataContext, IMapper<TDomainEntity, TDalEntity> mapper) : base(dataContext,
        mapper)
    {
    }
}

public class BaseEntityRepository<TKey, TDomainEntity, TDalEntity, TDbContext> : IBaseRepository<TDalEntity, TKey>
    where TDomainEntity : class, IEntityId<TKey>
    where TDalEntity : class, IEntityId<TKey>
    where TKey : struct, IEquatable<TKey>
    where TDbContext : DbContext
{
    protected readonly IMapper<TDomainEntity, TDalEntity> Mapper;

    // protected means its accessible in this class and in classes that inherit from this class
    protected TDbContext RepositoryDbContext;
    protected DbSet<TDomainEntity> RepositoryDbSet;

    public BaseEntityRepository(TDbContext dataContext, IMapper<TDomainEntity, TDalEntity> mapper)
    {
        RepositoryDbContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        Mapper = mapper;
        RepositoryDbSet = RepositoryDbContext.Set<TDomainEntity>();
    }


    // virtual means that the method can be overridden in a derived class

    public virtual async Task<TDalEntity?> FindAsync(TKey id, TKey? userId = default, bool noTracking = true)
    {
        var test =Mapper.Map(await CreateQuery(userId, noTracking)
            .FirstOrDefaultAsync(e => e.Id.Equals(id)));
        return test;
    }


    public virtual IEnumerable<TDalEntity> GetAll(TKey userId = default, bool noTracking = true)
    {
        return CreateQuery(userId, noTracking).ToList().Select(e => Mapper.Map(e))!;
    }

    public virtual async Task<IEnumerable<TDalEntity>> GetAllAsync(TKey userId = default, bool noTracking = true)
    {
        return (await CreateQuery(userId, noTracking).ToListAsync()).Select(e => Mapper.Map(e))!;
    }


    public virtual TDalEntity Add(TDalEntity entity)
    {
        return Mapper.Map(RepositoryDbSet.Add(Mapper.Map(entity)!).Entity)!;
    }

    public virtual TDalEntity Update(TDalEntity entity)
    {
        return Mapper.Map(RepositoryDbSet.Update(Mapper.Map(entity)!).Entity)!;
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