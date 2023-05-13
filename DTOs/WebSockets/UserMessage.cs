namespace Application.DTOs.WebSockets;

public class UserMessage : IWebSocketMessage
{
    public string MessageType { get; } = "UserMessage";
    public string SenderUserId { get; set; }
    public string RecipientUserId { get; set; }
    public string Message { get; set; }
}