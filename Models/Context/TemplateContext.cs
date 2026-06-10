namespace DotNetApiTemplate.Models.Context;

using DotNetApiTemplate.Models.Entities;
using DotNetApiTemplate.Models.EntityLogs;
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

    }
}


