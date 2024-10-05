using App.Contracts.DAL;
using App.Contracts.DAL.Repositories;
using App.DAL.EF.Repositories;
using AutoMapper;
using Base.DAL.EF;

namespace App.DAL.EF;

public class AppUnitOfWork : BaseUnitOfWork<AppDbContext>, IAppUnitOfWork
{
    private readonly IMapper _mapper; 
    private IHotelRepository? _hotelRepository;
    private IRoomRepository? _roomRepository;
    private IBookingRepository? _bookingRepository;

    
    public AppUnitOfWork(AppDbContext dbContext, IMapper mapper) : base(dbContext)
    {
        _mapper = mapper;
    }
    
    //lazy loading, initialise only when needed
    public IHotelRepository HotelRepository =>
        _hotelRepository ??= new HotelRepository(UowDbContext, _mapper);

    public IRoomRepository RoomRepository =>
        _roomRepository ??= new RoomRepository(UowDbContext, _mapper);

    public IBookingRepository BookingRepository =>
        _bookingRepository ??= new BookingRepository(UowDbContext, _mapper);
    
}

    