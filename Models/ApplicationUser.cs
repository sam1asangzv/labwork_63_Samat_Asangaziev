using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MyChat.Models;

public class ApplicationUser : IdentityUser
{
    [Required] public DateOnly BirthDate { get; set; }
    [Required] public string AvatarPath { get; set; } = "/images/default-avatar.svg";
    public bool IsBlocked { get; set; }
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}

public class UserRole
{
    public string UserId { get; set; } = "";
    public string RoleId { get; set; } = "";
}
