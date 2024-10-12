using Base.Contracts;

namespace App.DTO.DAL;


public class Room : IEntityId
{
    public Guid Id { get; set; }
    public string RoomName { get; set; } = default!;
    public int RoomNumber { get; set; }
    public int BedCount { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public Guid HotelId { get; set; }
}
