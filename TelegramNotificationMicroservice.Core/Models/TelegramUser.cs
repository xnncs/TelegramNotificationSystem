namespace TelegramNotificationMicroservice.Core.Models;

public class TelegramUser
{
    public static TelegramUser Create(long telegramId, string? username)
    {
        return new TelegramUser
        {
            Id = Guid.NewGuid(),
            TelegramId = telegramId,
            Username = username
        };
    }
    
    public Guid Id { get; set; }
    public long TelegramId { get; set; }
    public string? Username { get; set; }
}