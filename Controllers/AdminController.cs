using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyChat.Data;
using MyChat.Models;

namespace MyChat.Controllers;

[Authorize(Roles = "admin")]
public class AdminController(UserManager<ApplicationUser> users, AppDbContext db) : Controller
{
    public async Task<IActionResult> Users() => View(await db.Users.OrderBy(x => x.UserName).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBlock(string id)
    {
        var user = await users.FindByIdAsync(id); if (user is null) return NotFound();
        if (await users.IsInRoleAsync(user, "admin")) return BadRequest();
        user.IsBlocked = !user.IsBlocked; await users.UpdateAsync(user); return RedirectToAction(nameof(Users));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(string id, string role)
    {
        if (role is not ("user" or "admin")) return BadRequest();
        var user = await users.FindByIdAsync(id); if (user is null) return NotFound();
        var oldRoles = await users.GetRolesAsync(user); await users.RemoveFromRolesAsync(user, oldRoles); await users.AddToRoleAsync(user, role);
        return RedirectToAction(nameof(Users));
    }

    [HttpGet] public IActionResult CreateUser() => View(new RegisterViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(RegisterViewModel model, string role = "user")
    {
        ModelState.Remove(nameof(model.Avatar));
        if (model.BirthDate > DateOnly.FromDateTime(DateTime.Today.AddYears(-18))) ModelState.AddModelError(nameof(model.BirthDate), "Возраст должен быть не менее 18 лет.");
        if (!ModelState.IsValid) return View(model);
        var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, EmailConfirmed = true, BirthDate = model.BirthDate, AvatarPath = "/images/default-avatar.svg" };
        var result = await users.CreateAsync(user, model.Password);
        if (!result.Succeeded) { foreach (var e in result.Errors) ModelState.AddModelError("", e.Description); return View(model); }
        await users.AddToRoleAsync(user, role is "admin" ? "admin" : "user"); return RedirectToAction(nameof(Users));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        var message = await db.ChatMessages.FindAsync(id); if (message is null) return NotFound();
        db.ChatMessages.Remove(message); await db.SaveChangesAsync(); return Json(new { deleted = true, id });
    }
}
