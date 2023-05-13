using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Application.ViewModels;
using Application.Models;
namespace Application.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AccountController
    (
        UserManager<User> userManager,
        SignInManager<User> signInManager
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
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

                    if (result.Succeeded) return RedirectToAction("Index", "Main");
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
        await _signInManager.SignOutAsync();

        return RedirectToAction("LoggedOut");
    }

    public IActionResult LoggedOut() => View();
}