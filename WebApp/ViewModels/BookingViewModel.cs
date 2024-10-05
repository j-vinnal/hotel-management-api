using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.ViewModels;

public class BookingViewModel
{
    public App.DTO.Public.v1.Booking Booking { get; set; } = default!;
    public SelectList? RoomSelectList { get; set; }
}