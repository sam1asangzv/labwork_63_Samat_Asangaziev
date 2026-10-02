using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyChat.Data;
using MyChat.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders(); builder.Logging.AddConsole();
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = false;
}).AddDefaultTokenProviders();
builder.Services.AddScoped<IUserStore<ApplicationUser>, IdentityStore>();
builder.Services.AddScoped<IRoleStore<IdentityRole>, RoleStore>();
builder.Services.ConfigureApplicationCookie(o => { o.LoginPath = "/Account/Login"; o.AccessDeniedPath = "/Account/AccessDenied"; });
var keyFolder = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "Keys"); Directory.CreateDirectory(keyFolder);
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keyFolder));

var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseStaticFiles(); app.UseRouting(); app.UseAuthentication(); app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Chat}/{action=Index}/{id?}");
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}
await SeedData.InitializeAsync(app.Services);
app.Run();
