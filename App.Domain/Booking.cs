using App.Domain.Identity;
using Base.Contracts.Domain;
using Base.Domain;

namespace App.Domain;

public class Booking : AuditableEntity, IDomainAppUser<AppUser>
{
    public Guid RoomId { get; set; }
    public Room? Room { get; set; }
    public Guid AppUserId { get; set; }
    public AppUser? User { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCancelled { get; set; } = false;
}