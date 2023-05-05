using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Application.Data;
using Application.ViewModels;
using Application.Services;

namespace Application.Controllers;

[Authorize]
public class MessagingController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IMessagingWebSocketService _messagingWebSocketService;

    public MessagingController
    (
        ApplicationDbContext context,
        IMessagingWebSocketService messagingWebSocketService
    )
    {
        _context = context;
        _messagingWebSocketService = messagingWebSocketService;
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

    [HttpGet]
    public async Task<IActionResult> WebsocketConnection()
    {
        Console.WriteLine("Websocket connection request.");

        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            Console.WriteLine("Websocket request received.");

            await _messagingWebSocketService.HandleWebSocketAsync(HttpContext, () => Task.CompletedTask);

            return Ok();
        }

        Console.WriteLine("Not a websocket request.");
        return BadRequest();
    }
}