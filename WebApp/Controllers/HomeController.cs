using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using Microsoft.AspNetCore.Identity;
using App.Domain.Identity;
using Microsoft.AspNetCore.Localization;

namespace WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<AppUser> _userManager;

    public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    /// <summary>
    /// Sets the language for the application.
    /// </summary>
    /// <param name="culture">The culture to set.</param>
    /// <param name="returnUrl">The URL to return to after setting the language.</param>
    /// <returns>A redirect to the return URL.</returns>
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(
                new RequestCulture(culture)
            ),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(2)
            }
        );
        return Redirect(returnUrl);
    }
    
    
}