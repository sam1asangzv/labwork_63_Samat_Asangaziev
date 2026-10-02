using System.ComponentModel.DataAnnotations;

namespace MyChat.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Укажите имя пользователя.")] public string UserName { get; set; } = "";
    [Required(ErrorMessage = "Укажите email."), EmailAddress(ErrorMessage = "Некорректный email.")] public string Email { get; set; } = "";
    [Required(ErrorMessage = "Укажите дату рождения."), DataType(DataType.Date)] public DateOnly BirthDate { get; set; }
    [Required(ErrorMessage = "Выберите аватар.")] public IFormFile? Avatar { get; set; }
    [Required(ErrorMessage = "Укажите пароль."), DataType(DataType.Password)] public string Password { get; set; } = "";
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Укажите username или email.")] public string Identifier { get; set; } = "";
    [Required(ErrorMessage = "Укажите пароль."), DataType(DataType.Password)] public string Password { get; set; } = "";
}

public class EditProfileViewModel
{
    [Required(ErrorMessage = "Укажите имя пользователя.")] public string UserName { get; set; } = "";
    [Required, EmailAddress(ErrorMessage = "Некорректный email.")] public string Email { get; set; } = "";
    [Required, DataType(DataType.Date)] public DateOnly BirthDate { get; set; }
    public IFormFile? Avatar { get; set; }
}
