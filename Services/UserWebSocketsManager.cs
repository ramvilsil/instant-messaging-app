using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Application.Services;

public class UserWebSocketsManager : IUserWebSocketsManager
{
    public ConcurrentDictionary<string, WebSocket> Sockets { get; } = new ConcurrentDictionary<string, WebSocket>();
}