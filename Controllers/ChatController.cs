using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyChat.Data;
using MyChat.Models;

namespace MyChat.Controllers;

[Authorize]
public class ChatController(AppDbContext db, UserManager<ApplicationUser> users) : Controller
{
    public async Task<IActionResult> Index()
    {
        var user = await users.GetUserAsync(User); if (user?.IsBlocked != false) return RedirectToAction("Logout", "Account");
        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> Messages()
    {
        var currentId = users.GetUserId(User);
        var messages = await db.ChatMessages.Include(x => x.User).OrderByDescending(x => x.SentAt).Take(30).OrderBy(x => x.SentAt)
            .Select(x => new { x.Id, x.Text, sentAt = x.SentAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss"), userId = x.UserId, username = x.User.UserName, avatar = x.User.AvatarPath, mine = x.UserId == currentId }).ToListAsync();
        return Json(messages);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(string text)
    {
        var user = await users.GetUserAsync(User); if (user is null || user.IsBlocked) return Unauthorized();
        text = text?.Trim() ?? "";
        if (text.Length is < 1 or > 150) return BadRequest(new { message = "Сообщение должно содержать от 1 до 150 символов." });
        var message = new ChatMessage { Text = text, UserId = user.Id };
        db.ChatMessages.Add(message); await db.SaveChangesAsync(); return Json(new { success = true, message.Id });
    }
}
