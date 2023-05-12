namespace Application.DTOs.WebSockets;

public class ActiveUsers : IWebSocketMessage
{
    public string MessageType { get; } = "ActiveUsers";
    public List<User> Users { get; set; }
}