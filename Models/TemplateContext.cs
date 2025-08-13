namespace DotNetApiTemplate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

// public class TemplateContext(DbContextOptions<TemplateContext> options) : DbContext(options)
public class TemplateContext(DbContextOptions<TemplateContext> options) : IdentityDbContext<User, Role, string>(options)
{
    //Tables
    public DbSet<Table1> Table1 { get; set; } = null!;
    public DbSet<User> User { get; set; } = null!;
    public DbSet<Role> Role { get; set; } = null!;

    //Logs Table
    public DbSet<Table1Log> Table1Log { get; set; } = null!;
    public DbSet<UserLog> UserLog { get; set; } = null!;
    // public DbSet<Log> Log { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        //Identity
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaim");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserToken");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogin");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaim");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRole");

    }
}