using App.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace WebApp.ViewModels;

/// <summary>
/// ViewModel for the Privacy page, containing user and user claims information.
/// </summary>
public class HomePrivacyVM
{
    /// <summary>
    /// Gets or sets the application user.
    /// </summary>
    public AppUser? AppUser { get; set; }

    /// <summary>
    /// Gets or sets the collection of user claims.
    /// </summary>
    public ICollection<IdentityUserClaim<Guid>>? AppUserClaims { get; set; }
}