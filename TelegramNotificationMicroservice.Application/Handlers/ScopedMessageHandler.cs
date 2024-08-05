using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramNotificationMicroservice.Core.Models;
using TelegramNotificationMicroservice.Persistence.Database;
using TelegramUpdater.UpdateContainer;
using TelegramUpdater.UpdateHandlers.Scoped.ReadyToUse;

namespace TelegramNotificationMicroservice.Application.Handlers;

public class ScopedMessageHandler : MessageHandler
{
    public ScopedMessageHandler(ApplicationDbContext dbContext, ILogger<ScopedMessageHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ScopedMessageHandler> _logger;
    
    
    protected override async Task HandleAsync(IContainer<Message> container)
    {
        if (container.Update.Type != MessageType.Text)
        {
            return;
        }

        Message message = container.Update;
        
        if (message.Text!.StartsWith('/'))
        {
            (string, string) result = GetCommandArgumentsObject(message.Text);
            string command = result.Item1;
            string args = result.Item2;

            await OnCommand(command, args, message);  
        }
        else
        {
            await OnTextMessage(message);
        }
    }

    private async Task OnTextMessage(Message message)
    {
        string response =
            "Thanks for your message, but this is a notification bot and we dont work with messages, see you latter";
        await ResponseAsync(response);
    }

    private static (string, string) GetCommandArgumentsObject(string messageText)
    {
        int spaceIndex = messageText.IndexOf(' ');
        if (spaceIndex < 0)
        {
            spaceIndex = messageText.Length;
        }

        string command = messageText[..spaceIndex].ToLower();
        string args = messageText[spaceIndex..].TrimStart();
        
        return (command, args);
    }
        
    private async Task OnCommand(string command, string args, Message message)
    {
        switch (command)
        {
            case "/start":
                await OnStartCommandAsync(args, message.Chat.Id, message.Chat.Username);
                break;
        }
    }
    
    private async Task OnStartCommandAsync(string args, long telegramUserId, string? username)
    {
        bool containsUser = await _dbContext.TelegramUsers.AnyAsync(u => u.TelegramId == telegramUserId);
        if (containsUser)
        {
            string response = "Welcome to out bot! \nYou are already registered";
            await ResponseAsync(response);
            return;
        }
        
        
        Guid userId = Guid.Parse(args);
        bool alreadyRegistered = await _dbContext.TelegramUsers.AnyAsync(u => u.Id == userId);
        if (alreadyRegistered)
        {
            if (username == null)
            {
                string responseOnNullUsername = "Welcome to out bot! \nYour account is already registered on telegram";
                await ResponseAsync(responseOnNullUsername);
                return;
            }
            
            string previousUsername = (await _dbContext.TelegramUsers.FirstOrDefaultAsync(u => u.Id == userId))?.Username
                              ?? throw new Exception("Some logic exception (it will never be invoked)");
            if (previousUsername == username)
            {
                string responseOnSameUsername = "Welcome to out bot! \nYour account is already registered on telegram";
                await ResponseAsync(responseOnSameUsername);
                return;
            }
            
            string responseOnDifferentUsername = $"Welcome to out bot! \nYour account is already registered on telegram with username {previousUsername}";
            await ResponseAsync(responseOnDifferentUsername);
            return;
        }

        TelegramUser user = TelegramUser.Create(telegramUserId, username);
        await using (IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                _dbContext.TelegramUsers.Add(user);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);
                
                string responseOnServerError = "Sorry something went wrong, try again latter";
                await ResponseAsync(responseOnServerError);
                return;
            }
        }
        
        string messageText = "Welcome to our bot! \nThank you for registration";
        await ResponseAsync(messageText);
    }
}