namespace App.DTO.Public.v1.Identity;

public class JWTResponse
{
    public string Jwt { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}