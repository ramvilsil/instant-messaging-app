using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels;

public class LoginViewModel
{
    [Required]
    public string Username { get; set; }

    [Required]

    public string Password { get; set; }

    [Required]
    public string ConfirmPassword { get; set; }
}