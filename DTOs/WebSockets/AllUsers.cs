namespace Application.DTOs.WebSockets;

public class AllUsers : IWebSocketMessage
{
    public string MessageType { get; } = "AllUsers";
    public List<User> Users { get; set; }
}