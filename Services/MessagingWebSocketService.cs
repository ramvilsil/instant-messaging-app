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

    public async Task HandleWebSocketAsync(HttpContext httpContext, Func<Task> next)
    {
        if (httpContext.WebSockets.IsWebSocketRequest)
        {
            WebSocket socket = await httpContext.WebSockets.AcceptWebSocketAsync();

            var currentUser = httpContext.User;
            string currentUserId = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);

            await AddWebSocketConnectionAsync(currentUserId, socket);

            await HandleWebSocketMessagesAsync(currentUserId, socket);

            Console.WriteLine("Started listening for messages.");
        }

        else await next();
    }

    public async Task AddWebSocketConnectionAsync(string currentUserId, WebSocket socket)
    {
        _sockets.TryAdd(currentUserId, socket);

        Console.WriteLine($"Adding Websocket connection - User Id: {currentUserId}\nSocket Hashcode: {socket.GetHashCode()}");

        await Task.CompletedTask;
    }

    public async Task RemoveWebSocketConnectionAsync(string currentUserId)
    {
        _sockets.TryRemove(currentUserId, out _);

        Console.WriteLine($"Removing Websocket connection - User Id: {currentUserId}\nSocket: {_sockets.FirstOrDefault(x => x.Key == currentUserId).Value}");

        await Task.CompletedTask;
    }

    public async Task HandleWebSocketMessagesAsync(string senderUserId, WebSocket socket)
    {
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!result.CloseStatus.HasValue)
        {
            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

            var messageObject = JsonConvert.DeserializeObject<Message>(receivedMessage);

            string messagePayload = messageObject.MessagePayload;

            string recipientUserId = messageObject.RecipientUserId;

            await SendMessageAsync(senderUserId, recipientUserId, messagePayload);

            result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }

    public async Task SendMessageAsync(string senderUserId, string recipientUserId, string messagePayload)
    {
        if (_sockets.TryGetValue(recipientUserId, out WebSocket socket) && socket.State == WebSocketState.Open)
        {
            string messagePayloadJson = JsonConvert.SerializeObject(messagePayload);

            var buffer = Encoding.UTF8.GetBytes(messagePayloadJson);

            // Sends to the socket of recipient user
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

            Console.WriteLine($"Sent '{messagePayload}' message from {senderUserId} to {recipientUserId}.");
        }
    }
}