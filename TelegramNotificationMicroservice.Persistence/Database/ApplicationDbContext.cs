using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TelegramNotificationMicroservice.Core.Models;
using TelegramNotificationMicroservice.Persistence.Configurations;

namespace TelegramNotificationMicroservice.Persistence.Database;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
        
        Database.EnsureCreated();
    }

    private readonly IConfiguration _configuration;
    
    
    public DbSet<TelegramUser> TelegramUsers { get; set; }
    public DbSet<TelegramNotification> TelegramNotifications { get; set; }
    
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = _configuration.GetConnectionString(nameof(ApplicationDbContext))
                                  ?? throw new Exception("Service error: connection string is not configured");

        optionsBuilder.UseNpgsql(connectionString)
            .EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TelegramUserConfiguration());
        modelBuilder.ApplyConfiguration(new TelegramNotificationConfiguration());
    }
}