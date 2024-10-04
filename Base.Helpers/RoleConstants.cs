using System.Collections.ObjectModel;

namespace Base.Helpers;

public static class RoleConstants
{
    public const string Admin = nameof(Admin);
    public const string Guest = nameof(Guest);

    public static IReadOnlyList<string> DefaultRoles { get; } = new ReadOnlyCollection<string>(new[]
    {
        Admin,
        Guest
    });
}