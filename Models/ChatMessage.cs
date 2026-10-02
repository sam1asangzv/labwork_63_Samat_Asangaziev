using System.ComponentModel.DataAnnotations;

namespace MyChat.Models;

public class ChatMessage
{
    public int Id { get; set; }
    [Required, StringLength(150, MinimumLength = 1)] public string Text { get; set; } = "";
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    [Required] public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
}
