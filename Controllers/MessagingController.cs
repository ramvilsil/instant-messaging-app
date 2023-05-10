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
    private readonly ApplicationDbContext _context;

    public MessagingController
    (
        ApplicationDbContext context
    )
    {
        _context = context;
    }

    [HttpGet]
    [Authorize]
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
    public async Task<IActionResult> Chat(string recipientUserId)
    {
        var recipientUser = await _context.Users.FindAsync(recipientUserId);

        // Current authorized user
        var senderUser = await _context.Users.FindAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));

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