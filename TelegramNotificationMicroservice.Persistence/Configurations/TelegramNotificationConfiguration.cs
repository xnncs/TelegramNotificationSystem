using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TelegramNotificationMicroservice.Core.Models;

namespace TelegramNotificationMicroservice.Persistence.Configurations;

public class TelegramNotificationConfiguration : IEntityTypeConfiguration<TelegramNotification>
{
    public void Configure(EntityTypeBuilder<TelegramNotification> builder)
    {
        builder.HasMany(x => x.UsersSentTo)
            .WithMany();
    }
}