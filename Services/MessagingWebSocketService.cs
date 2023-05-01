using System.Net.WebSockets;
using System.Collections.Concurrent;
using System.Text;
using System.Security.Claims;
using Newtonsoft.Json;
using Application.DTOs;

namespace Application.Services;

public class MessagingWebSocketService : IMessagingWebSocketService
{
    private readonly ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

    public async Task SendMessageAsync(string recipientId, string message)
    {
        if (_sockets.TryGetValue(recipientId, out WebSocket socket) && socket.State == WebSocketState.Open)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
    public async Task HandleWebSocketAsync(HttpContext context, Func<Task> next)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();
            var user = context.User;
            string userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            await AddWebSocketConnectionAsync(userId, socket);
            await HandleWebSocketMessagesAsync(userId, socket);
        }
        else
        {
            await next();
        }
    }

    public async Task AddWebSocketConnectionAsync(string userId, WebSocket socket)
    {
        _sockets.TryAdd(userId, socket);
        await Task.CompletedTask;
    }

    public async Task RemoveWebSocketConnectionAsync(string userId)
    {
        _sockets.TryRemove(userId, out _);
        await Task.CompletedTask;
    }

    public async Task HandleWebSocketMessagesAsync(string userId, WebSocket socket)
    {
        byte[] buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!result.CloseStatus.HasValue)
        {
            string messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var messageObj = JsonConvert.DeserializeObject<MessagePayload>(messageJson);

            if (!string.IsNullOrEmpty(messageObj?.RecipientId) && !string.IsNullOrEmpty(messageObj.Message))
            {
                await SendMessageAsync(messageObj.RecipientId, messageObj.Message);
            }

            result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await RemoveWebSocketConnectionAsync(userId);
        await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }
}