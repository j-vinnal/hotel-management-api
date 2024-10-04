using App.Domain;
using App.Domain.Identity;
using Base.Contracts.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid, IdentityUserClaim<Guid>, AppUserRole,
    IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Room> Rooms { get; set; } = default!;
    public DbSet<Booking> Bookings { get; set; } = default!;
    public DbSet<AppRefreshToken> AppRefreshTokens { get; set; } = default!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // let the initial stuff run, base conf
        base.OnModelCreating(modelBuilder);


        // disable cascade delete
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;


        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(e => typeof(IDomainAuditableEntity).IsAssignableFrom(e.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType)
                .Property<DateTime>(nameof(IDomainAuditableEntity.UpdatedAtDt))
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity(entityType.ClrType)
                .Property<DateTime>(nameof(IDomainAuditableEntity.CreatedAtDt))
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }

 public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        foreach (var entity in ChangeTracker.Entries().Where(e => e.State != EntityState.Deleted))
        {
            foreach (var prop in entity
                         .Properties
                         .Where(x => x.Metadata.ClrType == typeof(DateTime)))
            {
                if (prop.CurrentValue != null)
                {
                    prop.CurrentValue = ((DateTime)prop.CurrentValue).ToUniversalTime();
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}