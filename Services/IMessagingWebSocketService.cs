using System.Net.WebSockets;

namespace Application.Services;

public interface IMessagingWebSocketService
{
    Task HandleWebSocketAsync(HttpContext httpContext, Func<Task> next);
    Task AddWebSocketConnectionAsync(string userId, WebSocket socket);
    Task RemoveWebSocketConnectionAsync(string userId);
    Task HandleWebSocketMessagesAsync(string userId, WebSocket socket);
    Task SendMessageAsync(string senderId, string recipientId, string message);
}