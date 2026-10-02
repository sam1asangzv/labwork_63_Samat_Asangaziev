using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyChat.Models;

namespace MyChat.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<IdentityRole> Roles => Set<IdentityRole>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>().HasKey(x => x.Id);
        builder.Entity<ApplicationUser>().HasIndex(x => x.NormalizedUserName).IsUnique();
        builder.Entity<ApplicationUser>().HasIndex(x => x.NormalizedEmail).IsUnique();
        builder.Entity<IdentityRole>().HasKey(x => x.Id);
        builder.Entity<IdentityRole>().HasIndex(x => x.NormalizedName).IsUnique();
        builder.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });
        builder.Entity<ChatMessage>().HasOne(x => x.User).WithMany(x => x.Messages).HasForeignKey(x => x.UserId);
    }
}
