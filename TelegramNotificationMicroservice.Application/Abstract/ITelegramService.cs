using TelegramNotificationMicroservice.Core.Enums;

namespace TelegramNotificationMicroservice.Application.Abstract;

public interface ITelegramService
{
    Task SentNotificationToMany(List<Guid> ids, string message);
    Task<NotificationStatus> SentNotification(Guid id, string message);
}