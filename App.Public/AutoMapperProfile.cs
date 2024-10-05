using AutoMapper;

namespace App.Public;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<App.DTO.DAL.Hotel, App.DTO.Public.v1.Hotel>().ReverseMap();
        CreateMap<App.DTO.DAL.Room, App.DTO.Public.v1.Room>().ReverseMap();
        CreateMap<App.DTO.DAL.Booking, App.DTO.Public.v1.Booking>().ReverseMap();
    }
}