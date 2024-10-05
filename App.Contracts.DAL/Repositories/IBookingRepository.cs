using App.DTO.DAL;
using Base.Contracts.DAL;

namespace App.Contracts.DAL.Repositories;

public interface IBookingRepository : IBaseEntityRepository<Booking>, IBookingRepositoryCustom<Booking>
{
    // Define additional methods if needed
}

public interface IBookingRepositoryCustom<TEntity>
{
    //add here shared methods between repo and services
    Task<IEnumerable<TEntity>> GetAllSortedAsync(Guid userId = default, bool noTracking = true);
    Task<TEntity?> FindWithDetailsAsync(Guid id, Guid? userId = default, bool noTracking = true);
    Task<bool> IsRoomBookedAsync(Guid roomId, DateTime startDate, DateTime endDate);
    
}