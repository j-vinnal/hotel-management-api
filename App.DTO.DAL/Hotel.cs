using Base.Contracts;

namespace App.DTO.DAL;


public class Hotel : IEntityId
{

  public Guid Id { get; set; }
  public string Name { get; set; } = default!;
  public string Address { get; set; } = default!;
  public string PhoneNumber { get; set; } = default!;
  public string Email { get; set; } = default!;
  public Guid AppUserId { get; set; }
}