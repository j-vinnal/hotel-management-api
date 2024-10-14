using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using App.DTO.Public.v1;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace App.Test.Integration.api;
[Collection("NonParallel")]
public class RoomsControllerTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    private const string AdminEmail = "admin@hotelx.com";
    private const string AdminPassword = "Foo.Bar1";
    private const string GuestEmail = "guest@hotelx.com";
    private const string GuestPassword = "Guest.Pass1";

    public RoomsControllerTest(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task AnonimousCanViewAvailableRooms()
    {
        // Arrange: Define the date range for the room availability check
        var startDate = DateTime.UtcNow.AddDays(5);
        var endDate = DateTime.UtcNow.AddDays(10);

        // Act: Request available rooms for the specified period
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1.0/Rooms?startDate={startDate:2024-09-01}&endDate={endDate:2024-12-01}");
        var response = await _client.SendAsync(request);

        // Assert: Check response
        response.EnsureSuccessStatusCode();
        var rooms = await response.Content.ReadFromJsonAsync<List<Room>>();
        Assert.NotNull(rooms);
        Assert.True(rooms.Count > 0); 
    }
    


}
