using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyChat.Models;

namespace MyChat.Data;

public class IdentityStore(AppDbContext db) : IUserPasswordStore<ApplicationUser>, IUserEmailStore<ApplicationUser>, IUserRoleStore<ApplicationUser>
{
    public void Dispose() { }
    public Task<string> GetUserIdAsync(ApplicationUser u, CancellationToken c) => Task.FromResult(u.Id);
    public Task<string?> GetUserNameAsync(ApplicationUser u, CancellationToken c) => Task.FromResult(u.UserName);
    public Task SetUserNameAsync(ApplicationUser u, string? n, CancellationToken c) { u.UserName = n; return Task.CompletedTask; }
    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser u, CancellationToken c) => Task.FromResult(u.NormalizedUserName);
    public Task SetNormalizedUserNameAsync(ApplicationUser u, string? n, CancellationToken c) { u.NormalizedUserName = n; return Task.CompletedTask; }
    public async Task<IdentityResult> CreateAsync(ApplicationUser u, CancellationToken c) { db.Users.Add(u); return await Save(c); }
    public async Task<IdentityResult> UpdateAsync(ApplicationUser u, CancellationToken c) { db.Users.Update(u); return await Save(c); }
    public async Task<IdentityResult> DeleteAsync(ApplicationUser u, CancellationToken c) { db.Users.Remove(u); return await Save(c); }
    public Task<ApplicationUser?> FindByIdAsync(string id, CancellationToken c) => db.Users.FirstOrDefaultAsync(x => x.Id == id, c);
    public Task<ApplicationUser?> FindByNameAsync(string normalized, CancellationToken c) => db.Users.FirstOrDefaultAsync(x => x.NormalizedUserName == normalized, c);
    public Task SetPasswordHashAsync(ApplicationUser u, string? hash, CancellationToken c) { u.PasswordHash = hash; return Task.CompletedTask; }
    public Task<string?> GetPasswordHashAsync(ApplicationUser u, CancellationToken c) => Task.FromResult(u.PasswordHash);
    public Task<bool> HasPasswordAsync(ApplicationUser u, CancellationToken c) => Task.FromResult(u.PasswordHash is not null);
    public Task SetEmailAsync(ApplicationUser u, string? email, CancellationToken c) { u.Email = email; return Task.CompletedTask; }
    public Task<string?> GetEmailAsync(ApplicationUser u, CancellationToken c) => Task.FromResult(u.Email);
    public Task<bool> GetEmailConfirmedAsync(ApplicationUser u, CancellationToken c) => Task.FromResult(u.EmailConfirmed);
    public Task SetEmailConfirmedAsync(ApplicationUser u, bool value, CancellationToken c) { u.EmailConfirmed = value; return Task.CompletedTask; }
    public Task<ApplicationUser?> FindByEmailAsync(string normalized, CancellationToken c) => db.Users.FirstOrDefaultAsync(x => x.NormalizedEmail == normalized, c);
    public Task<string?> GetNormalizedEmailAsync(ApplicationUser u, CancellationToken c) => Task.FromResult(u.NormalizedEmail);
    public Task SetNormalizedEmailAsync(ApplicationUser u, string? email, CancellationToken c) { u.NormalizedEmail = email; return Task.CompletedTask; }
    public async Task AddToRoleAsync(ApplicationUser u, string normalizedRole, CancellationToken c) { var role = await db.Roles.FirstAsync(x => x.NormalizedName == normalizedRole, c); if (!await db.UserRoles.AnyAsync(x => x.UserId == u.Id && x.RoleId == role.Id, c)) db.UserRoles.Add(new UserRole { UserId = u.Id, RoleId = role.Id }); await db.SaveChangesAsync(c); }
    public async Task RemoveFromRoleAsync(ApplicationUser u, string normalizedRole, CancellationToken c) { var role = await db.Roles.FirstOrDefaultAsync(x => x.NormalizedName == normalizedRole, c); if (role is null) return; var row = await db.UserRoles.FindAsync([u.Id, role.Id], c); if (row is not null) { db.UserRoles.Remove(row); await db.SaveChangesAsync(c); } }
    public async Task<IList<string>> GetRolesAsync(ApplicationUser u, CancellationToken c) => await (from ur in db.UserRoles join r in db.Roles on ur.RoleId equals r.Id where ur.UserId == u.Id select r.Name!).ToListAsync(c);
    public async Task<bool> IsInRoleAsync(ApplicationUser u, string normalizedRole, CancellationToken c) => await (from ur in db.UserRoles join r in db.Roles on ur.RoleId equals r.Id where ur.UserId == u.Id && r.NormalizedName == normalizedRole select ur).AnyAsync(c);
    public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string normalizedRole, CancellationToken c) => await (from u in db.Users join ur in db.UserRoles on u.Id equals ur.UserId join r in db.Roles on ur.RoleId equals r.Id where r.NormalizedName == normalizedRole select u).ToListAsync(c);
    private async Task<IdentityResult> Save(CancellationToken c) { try { await db.SaveChangesAsync(c); return IdentityResult.Success; } catch (Exception e) { return IdentityResult.Failed(new IdentityError { Description = e.Message }); } }
}

public class RoleStore(AppDbContext db) : IRoleStore<IdentityRole>
{
    public void Dispose() { }
    public Task<string> GetRoleIdAsync(IdentityRole r, CancellationToken c) => Task.FromResult(r.Id);
    public Task<string?> GetRoleNameAsync(IdentityRole r, CancellationToken c) => Task.FromResult(r.Name);
    public Task SetRoleNameAsync(IdentityRole r, string? n, CancellationToken c) { r.Name = n; return Task.CompletedTask; }
    public Task<string?> GetNormalizedRoleNameAsync(IdentityRole r, CancellationToken c) => Task.FromResult(r.NormalizedName);
    public Task SetNormalizedRoleNameAsync(IdentityRole r, string? n, CancellationToken c) { r.NormalizedName = n; return Task.CompletedTask; }
    public async Task<IdentityResult> CreateAsync(IdentityRole r, CancellationToken c) { db.Roles.Add(r); await db.SaveChangesAsync(c); return IdentityResult.Success; }
    public async Task<IdentityResult> UpdateAsync(IdentityRole r, CancellationToken c) { db.Roles.Update(r); await db.SaveChangesAsync(c); return IdentityResult.Success; }
    public async Task<IdentityResult> DeleteAsync(IdentityRole r, CancellationToken c) { db.Roles.Remove(r); await db.SaveChangesAsync(c); return IdentityResult.Success; }
    public Task<IdentityRole?> FindByIdAsync(string id, CancellationToken c) => db.Roles.FirstOrDefaultAsync(x => x.Id == id, c);
    public Task<IdentityRole?> FindByNameAsync(string normalized, CancellationToken c) => db.Roles.FirstOrDefaultAsync(x => x.NormalizedName == normalized, c);
}
