using AutoMapper;

namespace App.BLL;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<App.DTO.DAL.Hotel, App.DTO.BLL.Hotel>().ReverseMap();
        CreateMap<App.DTO.DAL.Room, App.DTO.BLL.Room>().ReverseMap();
        CreateMap<App.DTO.DAL.Booking, App.DTO.BLL.Booking>().ReverseMap();
    }
}