using Microsoft.Extensions.DependencyInjection;
using TelegramNotificationMicroservice.Persistence.Database;

namespace TelegramNotificationMicroservice.Persistence.Extensions;

public static class ConfigurationExtensions
{
    public static IServiceCollection ConfigureDbContexts(this IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>();

        return services;
    }
}