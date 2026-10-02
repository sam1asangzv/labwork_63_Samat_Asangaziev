using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyChat.Models;

namespace MyChat.Controllers;

public class AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn, IWebHostEnvironment env) : Controller
{
    [HttpGet] public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (model.BirthDate > DateOnly.FromDateTime(DateTime.Today.AddYears(-18))) ModelState.AddModelError(nameof(model.BirthDate), "Регистрация разрешена только с 18 лет.");
        if (await users.FindByNameAsync(model.UserName) is not null) ModelState.AddModelError(nameof(model.UserName), "Пользователь с таким username уже существует.");
        if (await users.FindByEmailAsync(model.Email) is not null) ModelState.AddModelError(nameof(model.Email), "Пользователь с таким email уже существует.");
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser { UserName = model.UserName.Trim(), Email = model.Email.Trim(), BirthDate = model.BirthDate, AvatarPath = await SaveAvatar(model.Avatar!), EmailConfirmed = true };
        var result = await users.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(nameof(model.Password), Translate(error.Code));
            return View(model);
        }
        await users.AddToRoleAsync(user, "user");
        await signIn.SignInAsync(user, false);
        return RedirectToAction("Index", "Chat");
    }

    [HttpGet] public IActionResult Login() => View(new LoginViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = model.Identifier.Contains('@') ? await users.FindByEmailAsync(model.Identifier) : await users.FindByNameAsync(model.Identifier);
        if (user is null || !await users.CheckPasswordAsync(user, model.Password)) { ModelState.AddModelError("", "Неверный username/email или пароль."); return View(model); }
        if (user.IsBlocked) { ModelState.AddModelError("", "Пользователь заблокирован администратором."); return View(model); }
        await signIn.SignInAsync(user, false); return RedirectToAction("Index", "Chat");
    }

    [Authorize] public async Task<IActionResult> Logout() { await signIn.SignOutAsync(); return RedirectToAction(nameof(Login)); }
    public IActionResult AccessDenied() => View();

    private async Task<string> SaveAvatar(IFormFile file)
    {
        var folder = Path.Combine(env.WebRootPath, "uploads"); Directory.CreateDirectory(folder);
        var name = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        await using var stream = System.IO.File.Create(Path.Combine(folder, name)); await file.CopyToAsync(stream); return "/uploads/" + name;
    }

    private static string Translate(string code) => code switch
    {
        "PasswordTooShort" => "Пароль должен содержать не менее 6 символов.",
        "PasswordRequiresUpper" => "Пароль должен содержать минимум одну заглавную букву.",
        "PasswordRequiresLower" => "Пароль должен содержать минимум одну строчную букву.",
        "PasswordRequiresDigit" => "Пароль должен содержать минимум одну цифру.",
        "DuplicateUserName" => "Пользователь с таким username уже существует.",
        "DuplicateEmail" => "Пользователь с таким email уже существует.",
        _ => "Не удалось создать пользователя. Проверьте введённые данные."
    };
}
