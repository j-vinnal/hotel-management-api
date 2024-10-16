using System.Net.Http.Json;
using App.DAL.EF;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Booking = App.Domain.Booking;
using Room = App.Domain.Room;

namespace App.Test.Integration.api;

[Collection("NonParallel")]
public class RoomsControllerTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public RoomsControllerTest(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    [Fact]
    public async Task AnonymousCanViewAvailableRooms()
    {
        // Arrange: Define the date range for the room availability check
        var startDate = DateTime.UtcNow.AddDays(5);
        var endDate = DateTime.UtcNow.AddDays(100);

        // Create a scope to resolve the AppDbContext
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Define a new room
        var room = new Room
        {
            Id = Guid.NewGuid(),
            RoomNumber = 106,
            RoomName = "Test Suite",
            BedCount = 2,
            Price = 150,
            HotelId = Guid.Parse("5ac3a4e0-2c97-444f-88f8-a1fe7cbdf94b"),
        };

        // Add the room to the database
        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync();

        // Act: Request available rooms for the specified period
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/v1.0/Rooms?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}"
        );
        var response = await _client.SendAsync(request);

        // Assert: Check response
        response.EnsureSuccessStatusCode();
        var rooms = await response.Content.ReadFromJsonAsync<List<Room>>();
        Assert.NotNull(rooms);
        Assert.Contains(rooms, r => r.Id == room.Id);
    }

    [Fact]
    public async Task RoomIsNotAvailableWhenBooked()
    {
        // Arrange: Define the date range for the room availability check
        var startDate = DateTime.UtcNow.AddDays(5);
        var endDate = DateTime.UtcNow.AddDays(10);

        // Create a scope to resolve the AppDbContext
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Define a new booking for an existing room (room is seeded in the database from the JSON file)
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            RoomId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
            AppUserId = Guid.Parse("1c439aaf-10f3-4c7d-b884-740097bbdd7b"),
            StartDate = startDate,
            EndDate = endDate,
            IsCancelled = false,
        };

        // Add the booking to the database
        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync();

        // Act: Request available rooms for the specified period
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/v1.0/Rooms?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}"
        );
        var response = await _client.SendAsync(request);

        // Assert: Check response
        response.EnsureSuccessStatusCode();
        var rooms = await response.Content.ReadFromJsonAsync<List<Room>>();
        Assert.NotNull(rooms);

        // Check that the room with ID "a1b2c3d4-e5f6-7890-abcd-ef1234567890" is not available
        Assert.DoesNotContain(rooms, r => r.Id == Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"));
    }
}
