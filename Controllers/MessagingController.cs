using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Application.Data;
using Application.ViewModels;

namespace Application.Controllers;

[Authorize]
public class MessagingController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public MessagingController
    (
        ApplicationDbContext dbContext
    )
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Users()
    {
        var users = await _dbContext.Users.ToListAsync();

        var response = new UsersViewModel
        {
            Users = users
        };

        return View(response);
    }

    [HttpGet]
    public async Task<IActionResult> Chat(string recipientUserId)
    {
        var recipientUser = await _dbContext.Users.FindAsync(recipientUserId);

        // Current authorized user
        var senderUser = await _dbContext.Users.FindAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));

        if (recipientUser != null)
        {
            var response = new ChatViewModel
            {
                RecipientUserId = recipientUser.Id,
                RecipientUsername = recipientUser.UserName
            };

            return View(response);
        }

        return BadRequest();
    }
}