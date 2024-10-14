using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using App.DAL.EF;
using App.DTO.Public.v1;
using App.DTO.Public.v1.Identity;
using AutoMapper;
using Base.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Protocol;
using WebApp.Helpers;
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
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
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
        var response = await _client.PostAsJsonAsync("/api/v1.0/identity/Account/Login", new { email = AdminEmail, password = AdminPassword });
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
        var response = await _client.PostAsJsonAsync("/api/v1.0/identity/Account/Login", new { email = GuestEmail, password = GuestPassword });
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
        Assert.True(bookings.Count > 0); 
    }

    [Fact]
    public async Task GuestCannotCancelBookingOutsideAllowedPeriod()
    {
        // Arrange: Log in as a user and get JWT
        var jwt = await GetJwtForUser(GuestEmail, GuestPassword);

        // Create a scope to resolve the AppDbContext
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Retrieve the booking from the database
        var bookingId = Guid.Parse("e6f7a8b9-c0d1-2345-abcd-ef6789012345");
        var booking = await dbContext.Bookings.FindAsync(bookingId);
       
        Assert.NotNull(booking);

        // Check if the booking is within the allowed cancellation period
        var daysDifference = (DateTime.UtcNow.Date - booking.StartDate.Date).TotalDays;
       
        Assert.True(daysDifference > BookingConstants.CancellationDaysLimit);
        
        // Act: Cancel the booking
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1.0/Bookings/{bookingId}/cancel");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var response = await _client.SendAsync(request);
        
        
        // Assert: Check response
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task AdminCanEditAndCancelBookingIfOutsideAllowedPeriod()
    {
        // Arrange: Log in as a user and get JWT
        var jwt = await GetJwtForUser(AdminEmail, AdminPassword);

        // Create a scope to resolve the AppDbContext and the mapper
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        // Retrieve the booking from the database
        var bookingId = Guid.Parse("e6f7a8b9-c0d1-2345-abcd-ef6789012345");
        var booking = await dbContext.Bookings.FindAsync(bookingId);

        Assert.NotNull(booking);

        // Check if the booking is within the allowed cancellation period
        var daysDifference = (DateTime.UtcNow.Date - booking.StartDate.Date).TotalDays;
        Assert.True(daysDifference > BookingConstants.CancellationDaysLimit);

        // Map the booking entity to the DTO
        var bookingDto = mapper.Map<App.DTO.Public.v1.Booking>(booking);
        bookingDto.QuestId = booking.AppUserId;
        bookingDto.IsCancelled = true;

        // Act: Update the booking to set IsCancelled = true
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1.0/bookings/{bookingId}")
        {
            Content = JsonContent.Create(bookingDto)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        
        
        var response = await _client.SendAsync(request);

        // Assert: Check response
        response.EnsureSuccessStatusCode();
    }


    [Fact]
    public async Task GuestCanCancelBookingIfWithinAllowedPeriod()
    {
        
        _testOutputHelper.WriteLine("GuestCanCancelBookingIfWithinAllowedPeriod");
        // Arrange: Log in as a user and get JWT
        var jwt = await GetJwtForUser(GuestEmail, GuestPassword);

        // Create a scope to resolve the AppDbContext
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Retrieve the booking from the database
        var bookingId = Guid.Parse("b9c0d1e2-f3a4-5678-abcd-ef9012345678");
      
        var booking = await dbContext.Bookings.FindAsync(bookingId);
     
        Assert.NotNull(booking);

        // Check if the booking is within the allowed cancellation period
        var daysDifference = (DateTime.UtcNow.Date - booking.StartDate.Date).TotalDays;
         Assert.True(daysDifference <= BookingConstants.CancellationDaysLimit);
        
        // Act: Cancel the booking
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1.0/bookings/{bookingId}/cancel");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var response = await _client.SendAsync(request);

        // Assert: Check response
        response.EnsureSuccessStatusCode();
    }
    
   
    [Fact]
    public async Task GuestCanBookFreeRoomForSpecifiedPeriod()
    {
        // Arrange: Log in as a guest and get JWT
        var jwt = await GetJwtForUser(GuestEmail, GuestPassword);

        // Create a scope to resolve the AppDbContext
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Define the booking details
        var roomId = Guid.Parse("c3d4e5f6-7890-1234-abcd-ef3456789012"); 
        var startDate = DateTime.UtcNow.AddDays(300);
        var endDate = DateTime.UtcNow.AddDays(350);

        // Ensure the room is free for the specified period
        var isRoomBooked = await dbContext.Bookings.AnyAsync(b => b.RoomId == roomId && 
                                                                  !b.IsCancelled &&
                                                                  ((startDate >= b.StartDate && startDate <= b.EndDate) ||
                                                                   (endDate <= b.EndDate && endDate >= b.StartDate)));
        Assert.False(isRoomBooked, "Room is already booked for the specified period.");

        // Create the booking DTO
        var bookingDto = new Booking
        {
            RoomId = roomId,
            StartDate = startDate,
            EndDate = endDate,
            QuestId = Guid.Parse("1c439aaf-10f3-4c7d-b884-740097bbdd7b")
        };

        // Act: Book the room
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1.0/Bookings")
        {
            Content = JsonContent.Create(bookingDto)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var response = await _client.SendAsync(request);

        // Assert: Check response
        response.EnsureSuccessStatusCode();
        var createdBooking = await response.Content.ReadFromJsonAsync<Booking>();
        Assert.NotNull(createdBooking);
        Assert.Equal(roomId, createdBooking.RoomId);
        Assert.Equal(startDate, createdBooking.StartDate);
        Assert.Equal(endDate, createdBooking.EndDate);
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
