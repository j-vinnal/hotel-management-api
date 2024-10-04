using System.ComponentModel.DataAnnotations;
using Base.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace App.Domain.Identity;

[Index(nameof(PersonalCode), IsUnique = true)]
public class AppUser : IdentityUser<Guid>, IEntityId
{
    [MinLength(1)]
    [MaxLength(64)]
    public string FirstName { get; set; } = default!;
    [MinLength(1)]
    [MaxLength(64)]
    public string LastName { get; set; } = default!;
    [MinLength(1)]
    [MaxLength(64)]
    public string PersonalCode { get; set; } = default!;
    public Hotel? Hotel { get; set; }

}