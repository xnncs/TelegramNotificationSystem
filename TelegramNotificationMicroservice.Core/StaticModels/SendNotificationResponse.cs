using TelegramNotificationMicroservice.Core.Enums;

namespace TelegramNotificationMicroservice.Core.StaticModels;

public record SendNotificationResponse(Guid id, NotificationStatus StatusCode);