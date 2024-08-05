
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using TelegramNotificationMicroservice.Application.Abstract;
using TelegramNotificationMicroservice.Core.Enums;
using TelegramNotificationMicroservice.Core.Models;
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
    
    // TODO: realise this in better way (i think use streams in grpc is good practice for this)
    public async Task SentNotificationToMany(List<Guid> ids, string message)
    {
        foreach (Guid id in ids)
        {
            await SentNotification(id, message);
        }
    }
    public async Task<NotificationStatus> SentNotification(Guid id, string message)
    {
        TelegramUser? user = await _dbContext.TelegramUsers.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotificationStatus.NotDelivered;
        }

        await _botClient.SendTextMessageAsync(user.TelegramId, message);
        return NotificationStatus.Success;
    }
}