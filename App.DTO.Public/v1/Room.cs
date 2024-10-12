using System.ComponentModel.DataAnnotations;
using Base.Contracts;

namespace App.DTO.Public.v1;

public class Room : IEntityId
{
    public Guid Id { get; set; }
    public string RoomName { get; set; } = default!;
    public int RoomNumber { get; set; }

    [Range(minimum: 1, maximum: 3, ErrorMessageResourceType = typeof(Base.Resources.Attribute), ErrorMessageResourceName = "ValueBetween")]
    public int BedCount { get; set; }
    public decimal Price { get; set; }
    
    public string? ImageUrl { get; set; }
    
   // public string? HotelName { get; set; }
    public Guid HotelId { get; set; }
}
