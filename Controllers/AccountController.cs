using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Application.ViewModels;
using Application.Models;
using Application.Data;

namespace Application.Controllers;
public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ApplicationDbContext _context;

    public AccountController
    (
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ApplicationDbContext context
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Users()
    {
        var users = await _context.Users.ToListAsync();

        var response = new UsersViewModel
        {
            Users = users
        };

        return View(response);
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel loginViewModel)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(loginViewModel.Username);

            if (user != null && loginViewModel.Password == loginViewModel.ConfirmPassword)
            {
                var passwordCheck = await _userManager.CheckPasswordAsync(user, loginViewModel.Password);

                if (passwordCheck)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, false, false);

                    if (result.Succeeded)
                    {
                        user.IsOnline = true;

                        await _context.SaveChangesAsync();

                        return RedirectToAction("Users");
                    }
                }
            }

            return View(loginViewModel);
        }

        return View(loginViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogOut()
    {
        string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        var user = await _context.Users.FindAsync(userId);

        if (user != null)
        {
            user.IsOnline = false;

            await _context.SaveChangesAsync();

            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        return RedirectToAction("Index", "Home");
    }
}