using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TelegramNotificationMicroservice.Core.Models;

namespace TelegramNotificationMicroservice.Persistence.Configurations;

public class TelegramUserConfiguration : IEntityTypeConfiguration<TelegramUser>
{
    public void Configure(EntityTypeBuilder<TelegramUser> builder)
    {
        builder.Property(u => u.Id).ValueGeneratedNever();
        builder.Property(u => u.TelegramId).ValueGeneratedNever();

        builder.Property(u => u.Username).HasMaxLength(50);
    }
}