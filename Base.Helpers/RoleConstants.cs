using System.Collections.ObjectModel;

namespace Base.Helpers;

public static class RoleConstants
{
    public const string Admin = nameof(Admin);
    public const string Customer = nameof(Customer);

    public static IReadOnlyList<string> DefaultRoles { get; } = new ReadOnlyCollection<string>(new[]
    {
        Admin,
        Customer
    });
}