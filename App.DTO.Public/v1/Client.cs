namespace App.DTO.Public.v1;

public class Client
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PersonalCode { get; set; } = default!;
}