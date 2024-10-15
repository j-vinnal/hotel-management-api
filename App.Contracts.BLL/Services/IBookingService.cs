using App.Contracts.DAL.Repositories;
using App.DTO.BLL;
using Base.Contracts.DAL;

namespace App.Contracts.BLL.Services;

public interface IBookingService : IBaseEntityRepository<Booking>, IBookingRepositoryCustom<Booking>
{
    // Define additional methods if needed
    
    bool CanCancelBooking(App.DTO.BLL.Booking booking);
}
