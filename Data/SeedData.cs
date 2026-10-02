using Microsoft.AspNetCore.Identity;
using MyChat.Models;

namespace MyChat.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        foreach (var role in new[] { "user", "admin" }) if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole(role));
        await EnsureUser(users, "admin", "admin@mychat.local", "Admin1", "admin");
        await EnsureUser(users, "demo-user", "user@mychat.local", "User123", "user");
    }

    private static async Task EnsureUser(UserManager<ApplicationUser> manager, string name, string email, string password, string role)
    {
        var user = await manager.FindByNameAsync(name);
        if (user is null)
        {
            user = new ApplicationUser { UserName = name, Email = email, EmailConfirmed = true, BirthDate = new DateOnly(2000, 1, 1), AvatarPath = "/images/default-avatar.svg" };
            var result = await manager.CreateAsync(user, password);
            if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }
        if (!await manager.IsInRoleAsync(user, role)) await manager.AddToRoleAsync(user, role);
    }
}
