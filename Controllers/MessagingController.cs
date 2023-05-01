using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Application.Data;
using Application.ViewModels;
using Application.Services;

namespace Application.Controllers;

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
    public async Task<IActionResult> Connect()
    {
        Console.WriteLine("Connect attempt.");

        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _messagingWebSocketService.AddWebSocketConnectionAsync(userId, webSocket);
            await _messagingWebSocketService.HandleWebSocketMessagesAsync(userId, webSocket);

            return Ok();
        }
        else
        {
            return BadRequest("Not a WebSocket request.");
        }
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> Messages(string userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user != null)
        {
            var response = new MessagesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName
            };

            return View(response);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(string recipientId, string message)
    {
        var user = await _context.Users.FindAsync(recipientId);

        if (user != null)
        {
            await _messagingWebSocketService.SendMessageAsync(user.Id, message);
        }

        return RedirectToAction("Messages", new { userId = user.Id });
    }

}