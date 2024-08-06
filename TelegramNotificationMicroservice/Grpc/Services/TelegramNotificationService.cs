using Grpc.Core;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using TelegramNotificationMicroservice.Application.Abstract;
using TelegramNotificationMicroservice.Application.Services;
using TelegramNotificationMicroservice.Core.Enums;
using TelegramNotificationServiceApp;

namespace TelegramNotificationMicroservice.Grpc.Services;

public class
    TelegramNotificationService : TelegramNotificationServiceApp.TelegramNotificationService.
    TelegramNotificationServiceBase
{
    public TelegramNotificationService(ITelegramService telegramService)
    {
        _telegramService = telegramService;
    }

    private readonly ITelegramService _telegramService;

    public override async Task SendALotNotifications(SendALotNotificationRequest request,
        IServerStreamWriter<SendNotificationResponse> responseStream,
        ServerCallContext context)
    {
        List<Guid> validIds = new List<Guid>();
        foreach (string id in request.UserIds)
        {
            if (!Guid.TryParseExact(id, "D", out Guid userId))
            {
                SendNotificationResponse response = new SendNotificationResponse
                {
                    UserId = id,
                    StatusCode = NotificationStatusCodes.WrongRequestFormat
                };
                await responseStream.WriteAsync(response);
            }

            validIds.Add(userId);
        }

        List<Core.StaticModels.SendNotificationResponse> responses = await _telegramService.SentNotificationToMany(validIds, request.Message);

        foreach (Core.StaticModels.SendNotificationResponse response in responses)
        {
            SendNotificationResponse notificationResponse = new SendNotificationResponse()
            {
                UserId = response.id.ToString(),
                StatusCode = GetStatusCode(response.StatusCode)
            };
            await responseStream.WriteAsync(notificationResponse);
        }

    }

    public override async Task<SendNotificationResponse> SendNotification(SendNotificationRequest request, ServerCallContext context)
    {
        if (!Guid.TryParseExact(request.UserId, "D", out Guid userId))
        {
            return new SendNotificationResponse
            {
                StatusCode = NotificationStatusCodes.WrongRequestFormat
            };
        }
        
        Core.StaticModels.SendNotificationResponse response = await _telegramService.SentNotification(userId, request.Message);
        
        NotificationStatusCodes code = GetStatusCode(response.StatusCode);

        return new SendNotificationResponse
        {
            UserId = request.UserId,
            StatusCode = code
        };
    }

    private NotificationStatusCodes GetStatusCode(NotificationStatus status)
    {
        return status switch
        {
            NotificationStatus.Success => NotificationStatusCodes.Success,
            NotificationStatus.NotDelivered => NotificationStatusCodes.NoUserExists,
            _ => NotificationStatusCodes.ServerInternalError
        };
    }
}