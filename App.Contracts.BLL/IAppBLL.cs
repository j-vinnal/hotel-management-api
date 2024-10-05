using App.Contracts.BLL.Services;
using Base.Contracts.BLL;

namespace App.Contracts.BLL;

public interface IAppBLL : IBaseBLL
{
    IHotelService HotelService { get; }
    IRoomService RoomService { get; }
    IBookingService BookingService { get; }
}