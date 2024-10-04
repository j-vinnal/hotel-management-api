using App.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;

public class BookingViewModel
{
    public Booking Booking { get; set; } = default!;
    public SelectList? RoomSelectList { get; set; }
}
