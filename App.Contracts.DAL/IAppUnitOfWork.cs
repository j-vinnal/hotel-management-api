using App.Contracts.DAL.Repositories;
using Base.Contracts.DAL;

namespace App.Contracts.DAL;

public interface IAppUnitOfWork : IUnitOfWork
{
    //list of your repositories
    
    IHotelRepository HotelRepository { get; }

    IRoomRepository RoomRepository { get; }

    IBookingRepository BookingRepository { get; }
    
}