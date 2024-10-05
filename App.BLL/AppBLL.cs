
using App.BLL.Services;
using App.Contracts.BLL;
using App.Contracts.BLL.Services;
using App.Contracts.DAL;
using AutoMapper;
using Base.BLL;

namespace App.BLL;

public class AppBLL : BaseBLL<IAppUnitOfWork>, IAppBLL
{
    private readonly IMapper _mapper;
    private readonly IAppUnitOfWork _uow;
    
    private IHotelService? _hotelService;
    private IRoomService? _roomService;
    private IBookingService? _bookingService;
    
    public AppBLL(IAppUnitOfWork uoW, IMapper mapper) : base(uoW)
    {
        _mapper = mapper;
        _uow = uoW;
    }

    public IHotelService HotelService =>
        _hotelService ?? new HotelService(Uow, _mapper);

    public IRoomService RoomService =>
        _roomService ?? new RoomService(Uow, _mapper);

    public IBookingService BookingService =>
        _bookingService ?? new BookingService(Uow, _mapper);
}