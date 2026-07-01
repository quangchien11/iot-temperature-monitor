using Microsoft.EntityFrameworkCore;
using TemperatureApi.Models;

namespace TemperatureApi.Data;

/// <summary>
/// DbContext của Entity Framework Core - ánh xạ các Entity sang bảng SQL Server.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TemperatureLog> TemperatureLogs => Set<TemperatureLog>();
    public DbSet<AlertSetting> AlertSettings => Set<AlertSetting>();
    public DbSet<AlertLog> AlertLogs => Set<AlertLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cấu hình bảng TemperatureLogs
        modelBuilder.Entity<TemperatureLog>(e =>
        {
            e.ToTable("TemperatureLogs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Temperature).HasColumnType("float").IsRequired();
            e.Property(x => x.Humidity).HasColumnType("float").IsRequired();
            e.Property(x => x.CreatedAt).IsRequired();
            // Đánh index theo thời gian để truy vấn thống kê nhanh hơn.
            e.HasIndex(x => x.CreatedAt);
        });

        // Cấu hình bảng AlertSettings
        modelBuilder.Entity<AlertSetting>(e =>
        {
            e.ToTable("AlertSettings");
            e.HasKey(x => x.Id);
            e.Property(x => x.MaxTemperature).HasColumnType("float").IsRequired();
        });

        // Cấu hình bảng AlertLogs
        modelBuilder.Entity<AlertLog>(e =>
        {
            e.ToTable("AlertLogs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Message).HasMaxLength(255);
            e.HasIndex(x => x.CreatedAt);
        });

        // Seed cấu hình cảnh báo mặc định (ngưỡng 35°C, đang bật).
        modelBuilder.Entity<AlertSetting>().HasData(new AlertSetting
        {
            Id = 1,
            MaxTemperature = 35,
            IsActive = true,
            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0)
        });
    }
}
