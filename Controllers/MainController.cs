using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Application.ViewModels;
using Application.Services;

namespace Application.Controllers;

public class MainController : Controller
{
    private readonly ILogger<MainController> _logger;
    private readonly UsersService _usersService;

    public MainController
    (
        ILogger<MainController> logger,
        UsersService usersService
    )
    {
        _logger = logger;
        _usersService = usersService;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Privacy() => View();

    [HttpGet]
    [Authorize]
    public IActionResult ActiveUsers() => View();

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> UserProfile(string userId)
    {
        var user = await _usersService.GetUserByIdAsync(userId);

        var response = new UserViewModel
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email
        };

        return View(response);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> UserMessage(string userId)
    {
        var user = await _usersService.GetUserByIdAsync(userId);

        var currentUser = await _usersService.GetCurrentUserAsync();

        var response = new UserMessageViewModel
        {
            SenderUserId = currentUser.Id,
            RecepientUserId = user.Id,
            RecepientUserName = user.UserName,
            RecepientEmail = user.Email
        };

        return View(response);
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var response = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        };

        return View(response);
    }
}