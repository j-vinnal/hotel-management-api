using Base.Contracts.DAL;
using Microsoft.EntityFrameworkCore;

namespace Base.DAL.EF;

public abstract class BaseUnitOfWork<TDbContext> : IUnitOfWork
    where TDbContext : DbContext
{
    protected readonly TDbContext UowDbContext;

    protected BaseUnitOfWork(TDbContext uowDbContext)
    {
        UowDbContext = uowDbContext;
    }
    
    public virtual async Task<int> SaveChangesAsync()
    {
        return await UowDbContext.SaveChangesAsync();
    }
}