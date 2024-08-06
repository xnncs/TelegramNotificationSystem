using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramNotificationMicroservice.Application.Abstract;
using TelegramNotificationMicroservice.Application.Handlers;
using TelegramNotificationMicroservice.Application.Services;
using TelegramNotificationMicroservice.Grpc.Services;
using TelegramNotificationMicroservice.Persistence.Extensions;
using TelegramUpdater;
using TelegramUpdater.Hosting;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
IServiceCollection services = builder.Services;
ConfigurationManager configuration = builder.Configuration;

services.AddLogging();
services.AddGrpc();

services.AddScoped<ITelegramService, TelegramService>();

services.AddControllers();

services.ConfigureDbContexts();

ConfigureTelegramUpdater();


WebApplication app = builder.Build();

app.MapGrpcService<TelegramNotificationService>();
app.MapGet("/", () => 
    "Communication with gRPC endpoints must be made through a gRPC client..."
);

app.MapControllers();

app.Run();



void ConfigureTelegramUpdater()
{
    string token = configuration.GetSection("TelegramBotToken").Value ??
                   throw new Exception("Server error: no telegram bot token configured");

    TelegramBotClient client = new TelegramBotClient(token);

    services.AddHttpClient("TelegramBotClient").AddTypedClient<ITelegramBotClient>(httpClient => client);

    UpdaterOptions updaterOptions = new UpdaterOptions(maxDegreeOfParallelism: 10, 
        allowedUpdates: [UpdateType.Message, UpdateType.CallbackQuery]);

    services.AddTelegramUpdater(client, updaterOptions, botBuilder =>
    {
        botBuilder.AddDefaultExceptionHandler()
            .AddScopedUpdateHandler<ScopedMessageHandler, Message>();
    });
}