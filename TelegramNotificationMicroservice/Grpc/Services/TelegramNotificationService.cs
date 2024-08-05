using Grpc.Core;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using TelegramNotificationMicroservice.Application.Abstract;
using TelegramNotificationMicroservice.Application.Services;
using TelegramNotificationMicroservice.Core.Enums;
using TelegramNotificationServiceApp;

namespace TelegramNotificationMicroservice.Grpc.Services;

public class TelegramNotificationService : TelegramNotificationServiceApp.TelegramNotificationService.TelegramNotificationServiceBase
{
    public TelegramNotificationService(ITelegramService telegramService)
    {
        _telegramService = telegramService;
    }

    private readonly ITelegramService _telegramService;

    public override async Task<SendNotificationResponse> SendNotification(SendNotificationRequest request, ServerCallContext context)
    {
        if (!Guid.TryParseExact(request.UserId, "D" ,out Guid userId))
        {
            return new SendNotificationResponse
            {
                StatusCode = NotificationStatusCodes.WrongRequestFormat
            };
        }
        
        NotificationStatus status = await _telegramService.SentNotification(userId, request.Message);

        NotificationStatusCodes code = status switch
        {
            NotificationStatus.Success => NotificationStatusCodes.Success,
            NotificationStatus.NotDelivered => NotificationStatusCodes.NoUserExists,
            _ => NotificationStatusCodes.ServerInternalError
        };

        return new SendNotificationResponse
        {
            StatusCode = code
        };
    }
}