using Base.Contracts;

namespace App.DTO.Public.v1;

public class Room : IEntityId
{
    public Guid Id { get; set; }
    public int RoomNumber { get; set; }
    public int BedCount { get; set; }
    public decimal Price { get; set; }
    
    public string? HotelName { get; set; }
    public Guid HotelId { get; set; }
}
