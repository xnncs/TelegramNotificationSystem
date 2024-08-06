using TelegramNotificationMicroservice.Core.Enums;
using TelegramNotificationMicroservice.Core.StaticModels;

namespace TelegramNotificationMicroservice.Application.Abstract;

public interface ITelegramService
{
    /// <summary>
    /// Works only with valid ids.
    /// </summary>
    Task<List<SendNotificationResponse>> SentNotificationToMany(List<Guid> ids, string message);
    Task<SendNotificationResponse> SentNotification(Guid id, string message);
}