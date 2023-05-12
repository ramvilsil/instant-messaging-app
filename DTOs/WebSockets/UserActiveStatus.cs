namespace Application.DTOs.WebSockets;

public class UserActiveStatus : IWebSocketMessage
{
    public string MessageType { get; } = "UserActiveStatus";
    public bool UserIsActive { get; set; }
}