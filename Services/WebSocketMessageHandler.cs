using System.Net.WebSockets;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Application.DTOs.WebSockets;
using Application.Data;

namespace Application.Services;

public class WebSocketMessageHandler
{
    private readonly IUserWebSocketsManager _userWebSocketsManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly UsersService _usersService;

    public WebSocketMessageHandler
    (
        IUserWebSocketsManager userWebSocketsManager,
        ApplicationDbContext dbContext,
        UsersService usersService
    )
    {
        _userWebSocketsManager = userWebSocketsManager;
        _dbContext = dbContext;
        _usersService = usersService;
    }

    public async Task HandleWebSocketMessagesAsync(string userId, WebSocket webSocket)
    {
        var buffer = new ArraySegment<byte>(new byte[1024 * 4]);
        WebSocketReceiveResult result;

        while (webSocket.State == WebSocketState.Open)
        {
            result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                Console.WriteLine($"Message received from {userId}");

                var messageJson = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, result.Count);

                var messageDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(messageJson);

                if (messageDict.ContainsKey("MessageType"))
                {
                    switch (messageDict["MessageType"])
                    {
                        case "UserActiveStatus":
                            var userActiveStatus = JsonConvert.DeserializeObject<UserActiveStatus>(messageJson);
                            Console.WriteLine($"User activity status: {userActiveStatus.UserIsActive}");

                            switch (userActiveStatus.UserIsActive)
                            {
                                case true:
                                    await SendMessageToUserAsync(userId,
                                        new
                                        {
                                            MessageType = "UserActiveStatus",
                                            UserIsActive = true
                                        }
                                    );
                                    break;

                                case false:
                                    await SendMessageToUserAsync(userId,
                                        new
                                        {
                                            MessageType = "UserActiveStatus",
                                            UserIsActive = false
                                        }
                                    );
                                    RemoveWebSocketConnection(userId);
                                    break;
                            }

                            await BroadcastActiveUsersAsync();

                            break;

                        case "UserMessage":
                            var userMessage = JsonConvert.DeserializeObject<UserMessage>(messageJson);

                            Console.WriteLine($"Chat from {userId} to {userMessage.RecipientUserId} - {userMessage.Message}");

                            await SendMessageToUserAsync(userMessage.RecipientUserId,
                                new
                                {
                                    MessageType = "UserMessage",
                                    SenderUserId = userMessage.SenderUserId,
                                    RecepientUserId = userMessage.RecipientUserId,
                                    Message = userMessage.Message
                                }
                            );

                            break;

                        case "User":
                            var user = JsonConvert.DeserializeObject<DTOs.WebSockets.User>(messageJson);
                            Console.WriteLine($"Username: {user.UserName}");
                            break;

                        case "ActiveUsers":
                            var activeUsers = JsonConvert.DeserializeObject<ActiveUsers>(messageJson);
                            Console.WriteLine($"Active Users: {activeUsers.Users.Count}");

                            await SendMessageToAllUsersAsync(activeUsers);

                            break;

                        default:
                            Console.WriteLine($"Unknown message type: {messageDict["MessageType"]}");
                            break;
                    }
                }
            }
        }
    }

    public async Task SendMessageToUserAsync(string userId, object message)
    {
        if (_userWebSocketsManager.Sockets.TryGetValue(userId, out WebSocket socket) && socket.State == WebSocketState.Open)
        {
            string messageJson = JsonConvert.SerializeObject(message);

            var buffer = Encoding.UTF8.GetBytes(messageJson);

            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public async Task SendMessageToAllUsersAsync(object message)
    {
        Console.WriteLine($"Sending message to all users: {message}");

        foreach (var webSocket in _userWebSocketsManager.Sockets)
        {
            if (webSocket.Value.State == WebSocketState.Open)
            {
                string messageJson = JsonConvert.SerializeObject(message);

                var buffer = Encoding.UTF8.GetBytes(messageJson);

                Console.WriteLine($"Sending {message} to {webSocket.Key}");

                await webSocket.Value.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }

    public async Task BroadcastActiveUsersAsync()
    {
        var activeUsers = new List<DTOs.WebSockets.User>();

        foreach (var webSocket in _userWebSocketsManager.Sockets)
        {
            if (webSocket.Value.State == WebSocketState.Open)
            {
                var activeUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == webSocket.Key);

                if (activeUser != null) activeUsers.Add(
                    new DTOs.WebSockets.User
                    {
                        UserId = activeUser.Id,
                        UserName = activeUser.UserName,
                        Email = activeUser.Email
                    }
                );
            }
        }

        var message = new ActiveUsers
        {
            Users = activeUsers
        };

        await SendMessageToAllUsersAsync(message);
    }

    public async Task HandleWebSocketAsync(HttpContext httpContext, Func<Task> next)
    {
        if (httpContext.WebSockets.IsWebSocketRequest)
        {
            WebSocket webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();

            Console.WriteLine("WebSocket connection accepted");

            var currentUser = await _usersService.GetCurrentUserAsync();

            AddWebSocketConnection(currentUser.Id, webSocket);

            await HandleWebSocketMessagesAsync(currentUser.Id, webSocket);

        }
        else await next();
    }

    public void AddWebSocketConnection(string userId, WebSocket webSocket)
    {
        _userWebSocketsManager.Sockets.TryAdd(userId, webSocket);
    }

    public void RemoveWebSocketConnection(string userId)
    {
        Console.WriteLine($"Removing WebSocket connection for {userId}");
        _userWebSocketsManager.Sockets.TryRemove(userId, out _);
    }
}