namespace TelegramNotificationMicroservice.Core.Models;

public class TelegramNotification
{
    public List<TelegramUser> UsersSentTo { get; set; }
    public string Message { get; set; }
}