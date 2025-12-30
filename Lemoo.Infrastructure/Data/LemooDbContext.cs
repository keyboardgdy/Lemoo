using Lemoo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lemoo.Infrastructure.Data;

/// <summary>
/// Lemoo应用程序数据库上下文
/// </summary>
public class LemooDbContext : DbContext
{
    public LemooDbContext(DbContextOptions<LemooDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// 用户表
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// 配置模型
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置User实体
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            // 用户名配置
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(100);
            entity.HasIndex(e => e.Username).IsUnique();
            
            // 密码哈希配置
            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);
            
            // 用户名称配置
            entity.Property(e => e.Name)
                .HasMaxLength(100);
            
            // 手机号配置
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20);
            entity.HasIndex(e => e.PhoneNumber)
                .IsUnique()
                .HasFilter("[PhoneNumber] IS NOT NULL"); // 只对非空值创建唯一索引
            
            // 邮箱配置
            entity.Property(e => e.Email)
                .HasMaxLength(200);
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL"); // 只对非空值创建唯一索引
            
            // 头像地址配置
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500);
            
            // 时间字段配置
            entity.Property(e => e.CreatedAt)
                .IsRequired();
            entity.Property(e => e.LastLoginAt);
            
            // 启用状态配置
            entity.Property(e => e.IsEnabled)
                .IsRequired()
                .HasDefaultValue(true);
        });
    }
}

