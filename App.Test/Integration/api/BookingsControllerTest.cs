using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using App.Constants;
using App.DAL.EF;
using App.DTO.Public.v1;
using App.DTO.Public.v1.Identity;
using AutoMapper;
using Base.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace App.Test.Integration.api;

[Collection("NonParallel")]
public class BookingsControllerTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _testOutputHelper;

    // Define constants for user credentials
    private const string AdminEmail = "admin@hotelx.com";
    private const string AdminPassword = "Foo.Bar1";
    private const string GuestEmail = "guest@hotelx.com";
    private const string GuestPassword = "Guest.Pass1";

    public BookingsControllerTest(CustomWebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task BookingsRequiresLogin()
    {
        // Act
        var response = await _client.GetAsync("/api/v1.0/Bookings");
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task IndexWithAdminUser()
    {
        // get jwt
        var response = await _client.PostAsJsonAsync(
            "/api/v1.0/identity/Account/Login",
            new { email = AdminEmail, password = AdminPassword }
        );
        var contentStr = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        var loginData = JsonSerializer.Deserialize<JWTResponse>(contentStr, JsonHelper.CamelCase);

        Assert.NotNull(loginData);
        Assert.NotNull(loginData.Jwt);
        Assert.True(loginData.Jwt.Length > 0);

        var msg = new HttpRequestMessage(HttpMethod.Get, "/api/v1.0/Bookings");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginData.Jwt);
        msg.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        response = await _client.SendAsync(msg);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task IndexWithGuestUser()
    {
        // get jwt
        var response = await _client.PostAsJsonAsync(
            "/api/v1.0/identity/Account/Login",
            new { email = GuestEmail, password = GuestPassword }
        );
        var contentStr = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        var loginData = JsonSerializer.Deserialize<JWTResponse>(contentStr, JsonHelper.CamelCase);

        Assert.NotNull(loginData);
        Assert.NotNull(loginData.Jwt);
        Assert.True(loginData.Jwt.Length > 0);

        var msg = new HttpRequestMessage(HttpMethod.Get, "/api/v1.0/Bookings");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginData.Jwt);
        msg.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        response = await _client.SendAsync(msg);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AdminCanViewAllBookings()
    {
        // Arrange: Log in as admin and get JWT
        var jwt = await GetJwtForUser(AdminEmail, AdminPassword);

        // Act: Request all bookings
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1.0/Bookings");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var response = await _client.SendAsync(request);

        // Assert: Check response
        response.EnsureSuccessStatusCode();
        var bookings = await response.Content.ReadFromJsonAsync<List<Booking>>();
        Assert.NotNull(bookings);

        // Get the expected count from the JSON file
        var expectedCount = GetBookingCountFromJson();
        Assert.True(
            bookings.Count >= expectedCount,
            $"Expected at least {expectedCount} bookings, but got {bookings.Count}."
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    // Add more InlineData attributes as needed, up to BookingCancellationDaysLimit - 1
    public async Task GuestCannotCancelBookingOutsideAllowedPeriod(int daysToAdd)
    {
        // Arrange: Log in as a guest and get JWT
        var jwt = await GetJwtForUser(GuestEmail, GuestPassword);

        // Create a scope to resolve the AppDbContext
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Define a new booking
        var booking = new App.Domain.Booking
        {
            Id = Guid.NewGuid(),
            RoomId = Guid.Parse("c3d4e5f6-7890-1234-abcd-ef3456789012"),
            AppUserId = Guid.Parse("1c439aaf-10f3-4c7d-b884-740097bbdd7b"),
            StartDate = DateTime.UtcNow.AddDays(daysToAdd),
            EndDate = DateTime.UtcNow.AddDays(daysToAdd + 10),
            IsCancelled = false,
        };

        Assert.True(booking.StartDate < DateTime.UtcNow.AddDays(BusinessConstants.BookingCancellationDaysLimit));

        // Add the booking to the database
        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync();

        // Act: Attempt to cancel the booking
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1.0/Bookings/{booking.Id}/cancel");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var response = await _client.SendAsync(request);

        // Assert: Check response
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GuestCanCancelBookingWithinAllowedPeriod(int daysToAdd)
    {
        // Arrange: Log in as a guest and get JWT
        var jwt = await GetJwtForUser(GuestEmail, GuestPassword);

        // Create a scope to resolve the AppDbContext
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Define a new booking
        var booking = new App.Domain.Booking
        {
            Id = Guid.NewGuid(),
            RoomId = Guid.Parse("d4e5f678-9012-3456-abcd-ef4567890123"),
            AppUserId = Guid.Parse("1c439aaf-10f3-4c7d-b884-740097bbdd7b"),
            StartDate = DateTime.UtcNow.Date.AddDays(daysToAdd + BusinessConstants.BookingCancellationDaysLimit),
            EndDate = DateTime.UtcNow.Date.AddDays(daysToAdd + BusinessConstants.BookingCancellationDaysLimit + 10),
            IsCancelled = false,
        };

        _testOutputHelper.WriteLine($"Booking Start Date: {booking.StartDate}");

        Assert.True(booking.StartDate >= DateTime.UtcNow.Date.AddDays(BusinessConstants.BookingCancellationDaysLimit));

        // Add the booking to the database
        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync();

        // Act: Attempt to cancel the booking
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1.0/Bookings/{booking.Id}/cancel");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var response = await _client.SendAsync(request);

        // Assert: Check response
        response.EnsureSuccessStatusCode();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task AdminCanEditAndCancelBookingIfOutsideAllowedPeriod(int daysToAdd)
    {
        // Arrange: Log in as admin and get JWT
        var jwt = await GetJwtForUser(AdminEmail, AdminPassword);

        // Create a scope to resolve the AppDbContext and the mapper
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        // Define a new booking
        var booking = new App.Domain.Booking
        {
            Id = Guid.NewGuid(),
            RoomId = Guid.Parse("d4e5f678-9012-3456-abcd-ef4567890123"),
            AppUserId = Guid.Parse("1c439aaf-10f3-4c7d-b884-740097bbdd7a"),
            StartDate = DateTime.UtcNow.Date.AddDays(BusinessConstants.BookingCancellationDaysLimit - daysToAdd),
            EndDate = DateTime.UtcNow.Date.AddDays(daysToAdd + BusinessConstants.BookingCancellationDaysLimit + 11),
            IsCancelled = false,
        };

        _testOutputHelper.WriteLine($"Booking Start Date: {booking.StartDate}");

        // Add the booking to the database
        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync();

        // Check if the booking is outside the allowed cancellation period
        var dateOnly = DateTime.UtcNow.Date;
        var canCancel = booking.StartDate.Date >= dateOnly.AddDays(BusinessConstants.BookingCancellationDaysLimit);
        Assert.False(canCancel);

        // Map the booking entity to the DTO
        var bookingDto = mapper.Map<Booking>(booking);
        bookingDto.QuestId = booking.AppUserId;
        bookingDto.IsCancelled = true;

        // Act: Update the booking to set IsCancelled = true
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1.0/bookings/{booking.Id}")
        {
            Content = JsonContent.Create(bookingDto),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        var response = await _client.SendAsync(request);

        // Assert: Check response
        response.EnsureSuccessStatusCode();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task GuestCannotEditAndCancelBookingIfOutsideAllowedPeriod(int daysToAdd)
    {
        // Arrange: Log in as admin and get JWT
        var jwt = await GetJwtForUser(GuestEmail, GuestPassword);

        // Create a scope to resolve the AppDbContext and the mapper
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        // Define a new booking
        var booking = new App.Domain.Booking
        {
            Id = Guid.NewGuid(),
            RoomId = Guid.Parse("d4e5f678-9012-3456-abcd-ef4567890123"),
            AppUserId = Guid.Parse("1c439aaf-10f3-4c7d-b884-740097bbdd7a"),
            StartDate = DateTime.UtcNow.Date.AddDays(BusinessConstants.BookingCancellationDaysLimit - daysToAdd),
            EndDate = DateTime.UtcNow.Date.AddDays(daysToAdd + BusinessConstants.BookingCancellationDaysLimit + 11),
            IsCancelled = false,
        };

        _testOutputHelper.WriteLine($"Booking Start Date: {booking.StartDate}");

        // Add the booking to the database
        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync();

        // Check if the booking is outside the allowed cancellation period
        var dateOnly = DateTime.UtcNow.Date;
        var canCancel = booking.StartDate.Date >= dateOnly.AddDays(BusinessConstants.BookingCancellationDaysLimit);
        Assert.False(canCancel);

        // Map the booking entity to the DTO
        var bookingDto = mapper.Map<Booking>(booking);
        bookingDto.QuestId = booking.AppUserId;
        bookingDto.IsCancelled = true;

        // Act: Update the booking to set IsCancelled = true
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1.0/bookings/{booking.Id}")
        {
            Content = JsonContent.Create(bookingDto),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        var response = await _client.SendAsync(request);

        // Assert: Check response
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GuestCanBookFreeRoomForSpecifiedPeriod()
    {
        // Arrange: Log in as a guest and get JWT
        var jwt = await GetJwtForUser(GuestEmail, GuestPassword);

        // Create a scope to resolve the AppDbContext
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Define a new room
        var room = new App.Domain.Room
        {
            Id = Guid.NewGuid(),
            RoomNumber = 999,
            RoomName = "Test Room",
            BedCount = 1,
            Price = 100,
            HotelId = Guid.Parse("5ac3a4e0-2c97-444f-88f8-a1fe7cbdf94b"),
        };

        // Add the room to the database
        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync();

        // Define the booking details
        var startDate = DateTime.UtcNow.AddDays(300).Date;
        var endDate = DateTime.UtcNow.AddDays(350).Date;

        // Ensure the room is free for the specified period
        var isRoomBooked = await dbContext.Bookings.AnyAsync(b =>
            b.RoomId == room.Id && !b.IsCancelled && b.StartDate.Date <= endDate && startDate <= b.EndDate.Date
        );

        Assert.False(isRoomBooked, "Room is already booked for the specified period.");

        // Create the booking DTO
        var bookingDto = new Booking
        {
            RoomId = room.Id,
            StartDate = startDate,
            EndDate = endDate,
            QuestId = Guid.Parse("1c439aaf-10f3-4c7d-b884-740097bbdd7b"),
        };

        // Act: Book the room
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1.0/Bookings")
        {
            Content = JsonContent.Create(bookingDto),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var response = await _client.SendAsync(request);

        // Assert: Check response
        response.EnsureSuccessStatusCode();
        var createdBooking = await response.Content.ReadFromJsonAsync<Booking>();
        Assert.NotNull(createdBooking);
        Assert.Equal(room.Id, createdBooking.RoomId);
        Assert.Equal(startDate, createdBooking.StartDate.Date);
        Assert.Equal(endDate, createdBooking.EndDate.Date);
    }

    [Fact]
    public async Task GuestCannotBookAlreadyBookedRoom()
    {
        // Arrange: Log in as a guest and get JWT
        var jwt = await GetJwtForUser(GuestEmail, GuestPassword);

        // Create a scope to resolve the AppDbContext
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Define a new room
        var room = new App.Domain.Room
        {
            Id = Guid.NewGuid(),
            RoomNumber = 1000,
            RoomName = "Test Room for Double Booking",
            BedCount = 1,
            Price = 100,
            HotelId = Guid.Parse("5ac3a4e0-2c97-444f-88f8-a1fe7cbdf94b"),
        };

        // Add the room to the database
        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync();

        // Define the first booking
        var startDate = DateTime.UtcNow.AddDays(10).Date;
        var endDate = DateTime.UtcNow.AddDays(15).Date;

        var firstBooking = new App.Domain.Booking
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            AppUserId = Guid.Parse("1c439aaf-10f3-4c7d-b884-740097bbdd7b"),
            StartDate = startDate,
            EndDate = endDate,
            IsCancelled = false,
        };

        // Add the first booking to the database
        dbContext.Bookings.Add(firstBooking);
        await dbContext.SaveChangesAsync();

        // Attempt to create a second booking for the same room and overlapping dates
        var secondBookingDto = new Booking
        {
            RoomId = room.Id,
            StartDate = startDate.AddDays(2), // Overlapping period
            EndDate = endDate.AddDays(2),
            QuestId = Guid.Parse("1c439aaf-10f3-4c7d-b884-740097bbdd7b"),
        };

        // Act: Try to book the room again
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1.0/Bookings")
        {
            Content = JsonContent.Create(secondBookingDto),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var response = await _client.SendAsync(request);

        // Assert: Check response
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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

    private int GetBookingCountFromJson()
    {
        var jsonFilePath = Path.Combine(
            _factory.Services.GetRequiredService<IWebHostEnvironment>().ContentRootPath,
            "..",
            "App.DAL.EF",
            "Seeding",
            "SeedData",
            "bookings.json"
        );
        var jsonData = File.ReadAllText(jsonFilePath);
        var bookings = JsonSerializer.Deserialize<List<Booking>>(jsonData);
        return bookings?.Count ?? 0;
    }
}
