using Competition.Domain.Entities;
using Competition.Domain.Entities.Person;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using System.Security.Cryptography.X509Certificates;

using Competition.Domain.Entities.MatchEntities;

namespace Competition.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly IClock _clock;

    public AppDbContext(DbContextOptions<AppDbContext> options, IClock? clock = null)
        : base(options)
    {
        _clock = clock ?? SystemClock.Instance;
    }
    
    public DbSet<RefreshToken> RefreshTokens { get; set; } 

    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<Referee> Referees { get; set; } = null!;
    public DbSet<Manager> Managers { get; set; } = null!;
    

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var now = _clock.GetCurrentInstant();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.ModifiedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Properties.Any(p => p.IsModified))
                    entry.Entity.ModifiedAt = now;
            }
            else if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.DeletedAt = now;
                entry.Entity.ModifiedAt = now;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>().ToTable("Persons");
        modelBuilder.Entity<Player>().ToTable("Players");
        modelBuilder.Entity<Manager>().ToTable("Managers");
        modelBuilder.Entity<Referee>().ToTable("Referees");

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.Id))
                    .HasDefaultValueSql("gen_random_uuid()");
            }
        }

        modelBuilder.Entity<MatchStatus>(e =>
        {
            e.ToTable("MatchStatuses");
            e.Property(x => x.Name).IsRequired().HasMaxLength(50);
            e.HasIndex(x => x.Slug).IsUnique().HasFilter("\"Slug\" IS NOT NULL");
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        modelBuilder.Entity<Match>(e =>
        {
            e.ToTable("Matches");
            e.HasIndex(x => x.Slug).IsUnique().HasFilter("\"Slug\" IS NOT NULL");
            e.HasOne(x => x.Status)
                .WithMany(s => s.Matches)
                .HasForeignKey(x => x.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Stadium)
                .WithMany()
                .HasForeignKey(x => x.StadiumId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });
    }

    public DbSet<Country> Countries { get; set; }
    public DbSet<GeoData> GeoDatas { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Person> Persons { get; set; } = null!;
    public DbSet<Stadium> Stadiums { get; set; }
    public DbSet<MatchStatus> MatchStatuses { get; set; } = null!;
    public DbSet<Match> Matches { get; set; } = null!;
    public DbSet<PlayerSelectionHistory> PlayerSelectionHistories { get; set; } = null!;
    public DbSet<SelectionManagerHistory> SelectionManagerHistories { get; set; } = null!;
    public DbSet<TransferValueHistory> TransferValueHistories { get; set; } = null!;
    public DbSet<MandatDate> MandatDates { get; set; } = null!;

}