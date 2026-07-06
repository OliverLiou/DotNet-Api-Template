namespace DotNetWebApiMssql.Models.Context;

using DotNetWebApiMssql.Models.Entities;
using DotNetWebApiMssql.Models.EntityLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

public class TemplateContext(DbContextOptions<TemplateContext> options) : IdentityDbContext<User, Role, string>(options)
{
    //Entities Table
    public DbSet<Table1> Table1 { get; set; } = null!;

    //EntityLogs Table
    public DbSet<Table1Log> Table1Log { get; set; } = null!;
    public DbSet<UserLog> UserLog { get; set; } = null!;
    public DbSet<UserRoleLog> UserRoleLog { get; set; } = null!;
    // public DbSet<Log> Log { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 自訂 Identity User 和 Role 的資料表名稱
        builder.Entity<User>(entity => { entity.ToTable("User"); });
        builder.Entity<Role>(entity => { entity.ToTable("Role"); });

        //Identity
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaim");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserToken");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogin");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaim");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRole");

        // 1. Seed Roles (Admin and User)
        var adminRoleId = "f32f3f98-e6b0-4de2-841d-926c04f14e30";
        var userRoleId = "8d2f33c3-3051-4e78-9ccf-8b9f71c4c1a5";
        builder.Entity<Role>().HasData(
            new Role { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN", RoleDesc = "系統管理員", ConcurrencyStamp = adminRoleId },
            new Role { Id = userRoleId, Name = "User", NormalizedName = "USER", RoleDesc = "一般使用者", ConcurrencyStamp = userRoleId }
        );

        // 2. Seed User (admin)
        var adminUserId = "37a7b8e5-7977-4b77-a8a5-dcd7b70bc904"; // GUID shared between User and UserRole
        var adminUser = new User
        {
            Id = adminUserId,
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            Email = "admin@example.com",
            NormalizedEmail = "ADMIN@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = "d8b5c92c-5609-4d22-b9cf-bd16cfb48f65",
            ConcurrencyStamp = adminUserId,
            EmployeeName = "System Admin",
            IsActive = true,
            CreatedAt = new DateTime(2026, 6, 22, 0, 0, 0, DateTimeKind.Utc), //避免被 EF Core 自動更新為當前時間
            LastLoginAt = null,
            PasswordHash = "AQAAAAIAAYagAAAAEGr3QbCm8Ad2r3N6jnkqkGYHrwroC3m3DIxpzRqa+9X92bcmb2S6S1gIdWzhYvXgpQ==" // 使用 BCrypt 加密的預設密碼 "Admin123!"
        };

        builder.Entity<User>().HasData(adminUser);

        // 3. Seed UserRole (Assign Admin role to admin user)
        builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
        {
            UserId = adminUserId,
            RoleId = adminRoleId
        });

        

    }
}


