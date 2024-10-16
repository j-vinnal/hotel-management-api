using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using App.DTO.Public.v1;
using App.DTO.Public.v1.Identity;
using Base.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace App.Test.Integration.api;

[Collection("NonParallel")]
public class HotelsControllerTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    private const string AdminEmail = "admin@hotelx.com";
    private const string AdminPassword = "Foo.Bar1";
    private const string GuestEmail = "guest@hotelx.com";
    private const string GuestPassword = "Guest.Pass1";

    public HotelsControllerTest(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    [Fact]
    public async Task AdminCanViewAllHotels()
    {
        // Arrange: Log in as admin and get JWT
        var jwt = await GetJwtForUser(AdminEmail, AdminPassword);

        // Act: Request all hotels
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1.0/Hotels");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var response = await _client.SendAsync(request);

        // Assert: Check response
        response.EnsureSuccessStatusCode();
        var hotels = await response.Content.ReadFromJsonAsync<List<Hotel>>();
        Assert.NotNull(hotels);
        Assert.True(hotels.Count > 0);
    }

    [Fact]
    public async Task GuestCannotViewAllHotels()
    {
        // Arrange: Log in as admin and get JWT
        var jwt = await GetJwtForUser(GuestEmail, GuestPassword);

        // Act: Request all hotels
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1.0/Hotels");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var response = await _client.SendAsync(request);

        // Assert: Check response

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<string> GetJwtForUser(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/v1.0/identity/Account/Login", new { email, password });
        response.EnsureSuccessStatusCode();

        var contentStr = await response.Content.ReadAsStringAsync();
        var loginData = JsonSerializer.Deserialize<JWTResponse>(contentStr, JsonHelper.CamelCase);

        if (loginData == null || string.IsNullOrEmpty(loginData.Jwt))
        {
            throw new InvalidOperationException("Failed to retrieve JWT");
        }

        return loginData.Jwt;
    }
}
