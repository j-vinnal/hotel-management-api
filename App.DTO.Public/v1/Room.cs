using System.ComponentModel.DataAnnotations;
using App.Constants;
using Base.Contracts;

namespace App.DTO.Public.v1;

public class Room : IEntityId
{
    public Guid Id { get; set; }
    public string RoomName { get; set; } = default!;
    public int RoomNumber { get; set; }

    [Range(minimum: BusinessConstants.MinRoomsPerHotel, maximum: BusinessConstants.MaxRoomsPerHotel, ErrorMessageResourceType = typeof(Base.Resources.Attribute), ErrorMessageResourceName = "ValueBetween")]
    public int BedCount { get; set; }
    public decimal Price { get; set; }
    
    public string? ImageUrl { get; set; }
    public Guid HotelId { get; set; }
}
