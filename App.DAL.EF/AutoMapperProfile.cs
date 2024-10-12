using AutoMapper;

namespace App.DAL.EF;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<App.Domain.Hotel, App.DTO.DAL.Hotel>().ReverseMap();

        CreateMap<App.Domain.Room, App.DTO.DAL.Room>().ReverseMap();
            
        CreateMap<App.Domain.Booking, App.DTO.DAL.Booking>()
            .ForMember(dest => dest.QuestId, opt => opt.MapFrom(src => src.AppUserId))
            .ReverseMap()
            .ForMember(dest => dest.AppUserId, opt => opt.MapFrom(src => src.QuestId));
    }
}