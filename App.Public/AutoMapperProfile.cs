using AutoMapper;

namespace App.Public;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<App.DTO.BLL.Hotel, App.DTO.Public.v1.Hotel>().ReverseMap();
        CreateMap<App.DTO.BLL.Room, App.DTO.Public.v1.Room>().ReverseMap();
        CreateMap<App.DTO.BLL.Booking, App.DTO.Public.v1.Booking>().ReverseMap();
    }
}