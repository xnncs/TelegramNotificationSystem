
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Telegram.Bot;
using TelegramNotificationMicroservice.Application.Abstract;
using TelegramNotificationMicroservice.Core.Enums;
using TelegramNotificationMicroservice.Core.Models;
using TelegramNotificationMicroservice.Core.StaticModels;
using TelegramNotificationMicroservice.Persistence.Database;

namespace TelegramNotificationMicroservice.Application.Services;

public class TelegramService : ITelegramService
{
    public TelegramService(ApplicationDbContext dbContext, ITelegramBotClient botClient)
    {
        _dbContext = dbContext;
        _botClient = botClient;
    }

    private readonly ApplicationDbContext _dbContext;
    private readonly ITelegramBotClient _botClient;
    

    /// <summary>
    /// Works only with valid ids.
    /// </summary>
    public async Task<List<SendNotificationResponse>> SentNotificationToMany(List<Guid> ids, string message)
    {
        await using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();
        
        List<SendNotificationResponse> responseGenerator = new List<SendNotificationResponse>();
        
        // sending notifications
        foreach (Guid id in ids)
        {
            TelegramUser? user = await _dbContext.TelegramUsers.FirstOrDefaultAsync(u => u.Id == id);
            if (user is null)
            {
                SendNotificationResponse response = new SendNotificationResponse(id, NotificationStatus.NotDelivered);
                responseGenerator.Add(response);
            }

            await _botClient.SendTextMessageAsync(user.TelegramId, message);
            SendNotificationResponse notificationResponse = new SendNotificationResponse(id, NotificationStatus.Success);
            responseGenerator.Add(notificationResponse);
        }

        // register notifications
        List<SendNotificationResponse> successNotificationResponses =
            responseGenerator.Where(x => x.StatusCode == NotificationStatus.Success).ToList();
        List<TelegramUser> usersThatReceivedNotification = await _dbContext.TelegramUsers.Where(
            u => successNotificationResponses.Select(x => x.id)
                .Contains(u.Id)).ToListAsync();

        if (usersThatReceivedNotification.Count != successNotificationResponses.Count)
        {
            throw new Exception("");
        }

        _dbContext.TelegramNotifications.Add(new TelegramNotification()
        {
            Message = message,
            UsersSentTo = usersThatReceivedNotification
        });

        await transaction.CommitAsync();
        
        return responseGenerator;
    }
    
    public async Task<SendNotificationResponse> SentNotification(Guid id, string message)
    {
        await using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();
        // sending notification
        TelegramUser? user = await _dbContext.TelegramUsers.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return new SendNotificationResponse(id, NotificationStatus.NotDelivered);
        }

        await _botClient.SendTextMessageAsync(user.TelegramId, message);
        
        
        // register notification in database
        TelegramNotification notification = new TelegramNotification()
        {
            UsersSentTo = [user],
            Message = message
        };
        _dbContext.Attach(user);
        _dbContext.TelegramNotifications.Add(notification);
        
        
        await transaction.CommitAsync();
        
        return new SendNotificationResponse(id, NotificationStatus.Success);
    }
}