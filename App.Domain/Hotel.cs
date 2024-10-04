using System.ComponentModel.DataAnnotations;
using App.Domain.Identity;
using Base.Contracts.Domain;
using Base.Domain;

namespace App.Domain;

public class Hotel : AuditableEntity, IDomainAppUser<AppUser>
{
    [MaxLength(256)] public string Name { get; set; } = default!;
    [MaxLength(256)] public string Address { get; set; } = default!;
    
    [Phone]
    public string PhoneNumber { get; set; } = default!;
    
    [EmailAddress]
    public string Email { get; set; } = default!;
    public Guid AppUserId { get; set; }
    public AppUser? User { get; set; }
    
    public ICollection<Room>? Rooms { get; set; }
}