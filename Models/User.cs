using Microsoft.AspNetCore.Identity;

namespace Application.Models;

public class User : IdentityUser
{
    public bool IsOnline { get; set; }
}