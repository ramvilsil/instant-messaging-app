namespace Application.DTOs.WebSockets;

public class User : IWebSocketMessage
{
    public string MessageType { get; } = "User";
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public bool UserActiveStatus { get; set; }
}