using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using App.DAL.EF;
using App.Domain.Identity;
using App.DTO.Public.v1;
using App.DTO.Public.v1.Identity;
using Asp.Versioning;
using Base.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Exceptions;

namespace WebApp.ApiControllers.Identity;

/// <summary>
/// API Controller for managing account-related operations.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("/api/v{version:apiVersion}/identity/[controller]/[action]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<AccountController> _logger;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;
    private readonly Random _rnd = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountController"/> class.
    /// </summary>
    /// <param name="userManager">The user manager for handling user-related operations.</param>
    /// <param name="logger">The logger instance for logging information.</param>
    /// <param name="signInManager">The sign-in manager for handling sign-in operations.</param>
    /// <param name="configuration">The configuration settings.</param>
    /// <param name="context">The database context.</param>
    public AccountController(
        UserManager<AppUser> userManager,
        ILogger<AccountController> logger,
        SignInManager<AppUser> signInManager,
        IConfiguration configuration,
        AppDbContext context
    )
    {
        _userManager = userManager;
        _logger = logger;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
    }

    /// <summary>
    /// Registers a new user and returns a JWT token.
    /// </summary>
    /// <param name="registrationData">The registration information.</param>
    /// <param name="expiresInSeconds">The expiration time for the JWT token in seconds.</param>
    /// <returns>A JWT response containing the token and refresh token.</returns>
    [HttpPost]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType<JWTResponse>((int)HttpStatusCode.OK)]
    [ProducesResponseType<RestApiErrorResponse>((int)HttpStatusCode.BadRequest)]
    [XRoadService("INSTANCE/CLASS/MEMBER/SUBSYSTEM/AccountService/Register")]
    public async Task<ActionResult<JWTResponse>> Register(
        [FromBody] RegisterInfo registrationData,
        [FromQuery] int expiresInSeconds
    )
    {
        const string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if (!Regex.IsMatch(registrationData.Email, emailPattern))
        {
            throw new BadRequestException("Invalid email format");
        }

        if (expiresInSeconds <= 0)
            expiresInSeconds = int.MaxValue;
        expiresInSeconds =
            expiresInSeconds < _configuration.GetValue<int>("JWT:expiresInSeconds")
                ? expiresInSeconds
                : _configuration.GetValue<int>("JWT:expiresInSeconds");

        var appUser = await _userManager.FindByEmailAsync(registrationData.Email);
        if (appUser != null)
        {
            throw new BadRequestException($"User with email {registrationData.Email} is already registered");
        }

        var refreshToken = new AppRefreshToken();
        appUser = new AppUser()
        {
            Email = registrationData.Email,
            UserName = registrationData.Email,
            FirstName = registrationData.Firstname,
            LastName = registrationData.Lastname,
            PersonalCode = registrationData.PersonalCode,
            RefreshTokens = new List<AppRefreshToken>() { refreshToken },
        };
        refreshToken.AppUser = appUser;

        var result = await _userManager.CreateAsync(appUser, registrationData.Password);
        if (!result.Succeeded)
        {
            throw new BadRequestException(result.Errors.First().Description);
        }

        var addToRoleResult = await _userManager.AddToRoleAsync(appUser, RoleConstants.Guest);
        if (!addToRoleResult.Succeeded)
        {
            throw new BadRequestException(addToRoleResult.Errors.First().Description);
        }

        result = await _userManager.AddClaimsAsync(
            appUser,
            new List<Claim>()
            {
                new(ClaimTypes.GivenName, appUser.FirstName),
                new(ClaimTypes.Surname, appUser.LastName),
                new("PersonalCode", registrationData.PersonalCode),
            }
        );

        if (!result.Succeeded)
        {
            throw new BadRequestException(result.Errors.First().Description);
        }

        appUser = await _userManager.FindByEmailAsync(appUser.Email);
        if (appUser == null)
        {
            throw new NotFoundException($"User with email {registrationData.Email} is not found after registration");
        }

        var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(appUser);
        var jwt = IdentityHelpers.GenerateJwt(
            claimsPrincipal.Claims,
            _configuration.GetValue<string>("JWT:key"),
            _configuration.GetValue<string>("JWT:issuer"),
            _configuration.GetValue<string>("JWT:audience"),
            expiresInSeconds
        );
        var res = new JWTResponse() { Jwt = jwt, RefreshToken = refreshToken.RefreshToken };
        return Ok(res);
    }

    /// <summary>
    /// Logs in a user and returns a JWT token.
    /// </summary>
    /// <param name="loginData">The login information.</param>
    /// <param name="expiresInSeconds">The expiration time for the JWT token in seconds.</param>
    /// <returns>A JWT response containing the token and refresh token.</returns>
    [HttpPost]
    [XRoadService("INSTANCE/CLASS/MEMBER/SUBSYSTEM/AccountService/Login")]
    public async Task<ActionResult<JWTResponse>> Login([FromBody] LoginInfo loginData, [FromQuery] int expiresInSeconds)
    {
        if (expiresInSeconds <= 0)
            expiresInSeconds = int.MaxValue;
        expiresInSeconds =
            expiresInSeconds < _configuration.GetValue<int>("JWT:expiresInSeconds")
                ? expiresInSeconds
                : _configuration.GetValue<int>("JWT:expiresInSeconds");

        // verify user
        var appUser = await _userManager.FindByEmailAsync(loginData.Email);
        if (appUser == null)
        {
            // Brute-force attack mitigation
            await Task.Delay(_rnd.Next(100, 1000));
            throw new NotFoundException("No account found with the provided email address");
        }
        // verify user
        var result = await _signInManager.CheckPasswordSignInAsync(appUser, loginData.Password, false);
        if (!result.Succeeded)
        {
            // Brute-force attack mitigation
            await Task.Delay(_rnd.Next(100, 1000));
            throw new NotFoundException("The password you entered is incorrect");
        }

        var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(appUser);
        if (claimsPrincipal == null)
        {
            // Brute-force attack mitigation
            await Task.Delay(_rnd.Next(100, 1000));
            throw new NotFoundException("Failed to generate user session");
        }

        if (!_context.Database.ProviderName!.Contains("InMemory"))
        {
            // Remove expired tokens directly in the database
            var deletedRows = await _context
                .AppRefreshTokens.Where(t =>
                    t.AppUserId == appUser.Id
                    && t.ExpirationDt < DateTime.UtcNow
                    && (t.PreviousExpirationDt == null || t.PreviousExpirationDt < DateTime.UtcNow)
                )
                .ExecuteDeleteAsync();

            _logger.LogInformation("Deleted {} refresh tokens", deletedRows);
        }

        var refreshToken = new AppRefreshToken() { AppUserId = appUser.Id };
        _context.AppRefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        var jwt = IdentityHelpers.GenerateJwt(
            claimsPrincipal.Claims,
            _configuration.GetValue<string>("JWT:key"),
            _configuration.GetValue<string>("JWT:issuer"),
            _configuration.GetValue<string>("JWT:audience"),
            expiresInSeconds
        );

        var responseData = new JWTResponse() { Jwt = jwt, RefreshToken = refreshToken.RefreshToken };
        return Ok(responseData);
    }

    /// <summary>
    /// Refreshes the JWT token using a refresh token.
    /// </summary>
    /// <param name="tokenRefreshData">The token refresh information.</param>
    /// <param name="expiresInSeconds">The expiration time for the new JWT token in seconds.</param>
    /// <returns>A JWT response containing the new token and refresh token.</returns>
    [HttpPost]
    [XRoadService("INSTANCE/CLASS/MEMBER/SUBSYSTEM/AccountService/RefreshTokenData")]
    public async Task<ActionResult<JWTResponse>> RefreshTokenData(
        [FromBody] TokenRefreshInfo tokenRefreshData,
        [FromQuery] int expiresInSeconds
    )
    {
        if (expiresInSeconds <= 0)
            expiresInSeconds = int.MaxValue;
        expiresInSeconds =
            expiresInSeconds < _configuration.GetValue<int>("JWT:expiresInSeconds")
                ? expiresInSeconds
                : _configuration.GetValue<int>("JWT:expiresInSeconds");

        JwtSecurityToken? jwt;

        try
        {
            jwt = new JwtSecurityTokenHandler().ReadJwtToken(tokenRefreshData.Jwt);
            if (jwt == null)
            {
                throw new BadRequestException("No token");
            }
        }
        catch (Exception)
        {
            throw new BadRequestException("Invalid token format");
        }

        if (
            !IdentityHelpers.ValidateJWT(
                tokenRefreshData.Jwt,
                _configuration.GetValue<string>("JWT:key"),
                _configuration.GetValue<string>("JWT:issuer"),
                _configuration.GetValue<string>("JWT:audience")
            )
        )
        {
            throw new BadRequestException("JWT validation fail");
        }

        var userEmail = jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        if (userEmail == null)
        {
            throw new BadRequestException("No email in jwt");
        }

        var appUser = await _userManager.FindByEmailAsync(userEmail);
        if (appUser == null)
        {
            throw new NotFoundException($"User with email {userEmail} not found");
        }

        await _context
            .Entry(appUser)
            .Collection(u => u.RefreshTokens!)
            .Query()
            .Where(x =>
                (x.RefreshToken == tokenRefreshData.RefreshToken && x.ExpirationDt > DateTime.UtcNow)
                || (x.PreviousRefreshToken == tokenRefreshData.RefreshToken && x.PreviousExpirationDt > DateTime.UtcNow)
            )
            .ToListAsync();

        if (appUser.RefreshTokens == null || appUser.RefreshTokens.Count == 0)
        {
            throw new NotFoundException("RefreshTokens collection is null or empty");
        }

        if (appUser.RefreshTokens.Count != 1)
        {
            throw new NotFoundException("More than one valid refresh token found");
        }

        var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(appUser);
        if (claimsPrincipal == null)
        {
            _logger.LogWarning("Could not get ClaimsPrincipal for user {}", userEmail);
            throw new BadRequestException("User/Password problem");
        }

        var jwtResponseStr = IdentityHelpers.GenerateJwt(
            claimsPrincipal.Claims,
            _configuration.GetValue<string>("JWT:key"),
            _configuration.GetValue<string>("JWT:issuer"),
            _configuration.GetValue<string>("JWT:audience"),
            expiresInSeconds
        );

        var refreshToken = appUser.RefreshTokens.First();
        if (refreshToken.RefreshToken == tokenRefreshData.RefreshToken)
        {
            refreshToken.PreviousRefreshToken = refreshToken.RefreshToken;
            refreshToken.PreviousExpirationDt = DateTime.UtcNow.AddMinutes(1);

            refreshToken.RefreshToken = Guid.NewGuid().ToString();
            refreshToken.ExpirationDt = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();
        }

        var res = new JWTResponse() { Jwt = jwtResponseStr, RefreshToken = refreshToken.RefreshToken };
        return Ok(res);
    }

    /// <summary>
    /// Logs out a user by invalidating their refresh tokens.
    /// </summary>
    /// <param name="logout">The logout information containing the refresh token to invalidate.</param>
    /// <returns>The number of tokens deleted.</returns>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost]
    [XRoadService("INSTANCE/CLASS/MEMBER/SUBSYSTEM/AccountService/Logout")]
    public async Task<ActionResult> Logout([FromBody] LogoutInfo logout)
    {
        var userIdStr = _userManager.GetUserId(User) ?? throw new BadRequestException("Invalid refresh token");
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            throw new BadRequestException("Deserialization error");
        }

        var appUser = await _context.Users.Where(u => u.Id == userId).SingleOrDefaultAsync();
        if (appUser == null)
        {
            throw new NotFoundException("User/Password problem");
        }

        await _context
            .Entry(appUser)
            .Collection(u => u.RefreshTokens!)
            .Query()
            .Where(x => (x.RefreshToken == logout.RefreshToken) || (x.PreviousRefreshToken == logout.RefreshToken))
            .ToListAsync();

        foreach (var appRefreshToken in appUser.RefreshTokens!)
        {
            _context.AppRefreshTokens.Remove(appRefreshToken);
        }

        var deleteCount = await _context.SaveChangesAsync();
        return Ok(new { TokenDeleteCount = deleteCount });
    }
}
