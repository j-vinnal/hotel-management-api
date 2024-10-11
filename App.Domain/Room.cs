using Base.Domain;

namespace App.Domain;

public class Room: AuditableEntity

{
    public string RoomName { get; set; } = default!;
    public int RoomNumber { get; set; }
    public int BedCount { get; set; }
    public decimal Price { get; set; }
    public Guid HotelId { get; set; }
    
    public string? ImageUrl { get; set; }
    public Hotel? Hotel { get; set; }
}