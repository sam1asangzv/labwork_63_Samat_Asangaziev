using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyChat.Data;
using MyChat.Models;

namespace MyChat.Controllers;

[Authorize]
public class ProfileController(UserManager<ApplicationUser> users, AppDbContext db, IWebHostEnvironment env) : Controller
{
    public async Task<IActionResult> Details(string? id)
    {
        var user = id is null ? await users.GetUserAsync(User) : await users.FindByIdAsync(id);
        if (user is null) return NotFound();
        ViewBag.MessageCount = await db.ChatMessages.CountAsync(x => x.UserId == user.Id);
        ViewBag.IsOwn = users.GetUserId(User) == user.Id;
        return View(user);
    }

    [HttpGet] public async Task<IActionResult> Edit()
    {
        var user = await users.GetUserAsync(User); if (user is null) return NotFound();
        return View(new EditProfileViewModel { UserName = user.UserName!, Email = user.Email!, BirthDate = user.BirthDate });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        var user = await users.GetUserAsync(User); if (user is null) return NotFound();
        if (model.BirthDate > DateOnly.FromDateTime(DateTime.Today.AddYears(-18))) ModelState.AddModelError(nameof(model.BirthDate), "Возраст должен быть не менее 18 лет.");
        var sameName = await users.FindByNameAsync(model.UserName); if (sameName is not null && sameName.Id != user.Id) ModelState.AddModelError(nameof(model.UserName), "Username уже занят.");
        var sameEmail = await users.FindByEmailAsync(model.Email); if (sameEmail is not null && sameEmail.Id != user.Id) ModelState.AddModelError(nameof(model.Email), "Email уже занят.");
        if (!ModelState.IsValid) return View(model);
        user.UserName = model.UserName.Trim(); user.Email = model.Email.Trim(); user.BirthDate = model.BirthDate;
        if (model.Avatar is { Length: > 0 }) user.AvatarPath = await SaveAvatar(model.Avatar);
        await users.UpdateAsync(user); return RedirectToAction(nameof(Details));
    }

    private async Task<string> SaveAvatar(IFormFile file) { var folder = Path.Combine(env.WebRootPath, "uploads"); Directory.CreateDirectory(folder); var name = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}"; await using var stream = System.IO.File.Create(Path.Combine(folder, name)); await file.CopyToAsync(stream); return "/uploads/" + name; }
}
