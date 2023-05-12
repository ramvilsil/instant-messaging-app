using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Application.Services;

public interface IUserWebSocketsManager
{
    ConcurrentDictionary<string, WebSocket> Sockets { get; }
}