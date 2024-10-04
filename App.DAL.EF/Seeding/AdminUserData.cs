namespace App.DAL.EF.Seeding;

public class AdminUserData
{
    public string Id { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    
    public string PersonalCode { get; set; } = default!;
    public string Password { get; set; } = default!;
}