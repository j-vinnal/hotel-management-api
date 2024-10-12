using App.DTO.DAL;
using Base.Contracts.DAL;

namespace App.Contracts.DAL.Repositories;

public interface IRoomRepository : IBaseEntityRepository<Room>, IRoomRepositoryCustom<Room>
{
    // Define additional methods if needed
}

public interface IRoomRepositoryCustom<TEntity>
{
    //add here shared methods between repo and service

    Task<IEnumerable<TEntity>> GetAllSortedAsync(bool noTracking = true);
    Task<TEntity?> FindWithDetailsAsync(Guid id, bool noTracking = true);
    Task<IEnumerable<TEntity>> GetAvailableRoomsAsync(DateTime startDate, DateTime endDate, int guestCount, bool noTracking = true);
}