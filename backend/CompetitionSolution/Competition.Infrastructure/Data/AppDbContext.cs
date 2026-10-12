using Competition.Domain.Entities;
using Competition.Domain.Entities.Person;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using System.Security.Cryptography.X509Certificates;

using Competition.Domain.Entities.FormationEntities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Domain.Entities.CompetitionEntities;
using CompetitionEntity = Competition.Domain.Entities.CompetitionEntities.Competition;

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
            e.HasOne(x => x.HomeSelection)
                .WithMany(s => s.HomeMatches)
                .HasForeignKey(x => x.HomeSelectionId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.AwaySelection)
                .WithMany(s => s.AwayMatches)
                .HasForeignKey(x => x.AwaySelectionId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Round)
                .WithMany(r => r.Matches)
                .HasForeignKey(x => x.RoundId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        modelBuilder.Entity<CompetitionEntity>(e =>
        {
            e.ToTable("Competitions");
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Slug).IsRequired().HasMaxLength(120);
            e.Property(x => x.ShortName).HasMaxLength(20);
            e.Property(x => x.Description).HasMaxLength(1000);
            e.HasIndex(x => x.Slug).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            e.HasIndex(x => x.Name).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            e.HasOne(x => x.Country)
                .WithMany()
                .HasForeignKey(x => x.CountryId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        modelBuilder.Entity<Season>(e =>
        {
            e.ToTable("Seasons");
            e.Property(x => x.Name).IsRequired().HasMaxLength(50);
            e.Property(x => x.Slug).IsRequired().HasMaxLength(200);
            e.HasIndex(x => x.Slug).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            e.HasIndex(x => new { x.CompetitionId, x.Name }).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            e.HasOne(x => x.Competition)
                .WithMany(c => c.Seasons)
                .HasForeignKey(x => x.CompetitionId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        modelBuilder.Entity<Round>(e =>
        {
            e.ToTable("Rounds");
            e.Property(x => x.Name).IsRequired().HasMaxLength(50);
            e.Property(x => x.Slug).IsRequired().HasMaxLength(250);
            e.HasIndex(x => x.Slug).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            e.HasIndex(x => new { x.SeasonId, x.Number }).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            e.HasOne(x => x.Season)
                .WithMany(s => s.Rounds)
                .HasForeignKey(x => x.SeasonId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        modelBuilder.Entity<Selection>(e =>
        {
            e.ToTable("Selections");
            e.Property(x => x.Name).IsRequired().HasMaxLength(120);
            e.Property(x => x.Abbreviation).IsRequired().HasMaxLength(10);
            e.HasIndex(x => x.Slug).IsUnique().HasFilter("\"Slug\" IS NOT NULL");
            e.HasOne(x => x.Country)
                .WithMany()
                .HasForeignKey(x => x.CountryId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        modelBuilder.Entity<PlayerSelectionHistory>(e =>
        {
            e.HasOne(x => x.Player)
                .WithMany()
                .HasForeignKey(x => x.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Selection)
                .WithMany(s => s.PlayerHistories)
                .HasForeignKey(x => x.SelectionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SelectionManagerHistory>(e =>
        {
            e.HasOne(x => x.Manager)
                .WithMany()
                .HasForeignKey(x => x.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Selection)
                .WithMany(s => s.ManagerHistories)
                .HasForeignKey(x => x.SelectionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FormationMark>(e =>
        {
            e.ToTable("FormationMarks");
            e.Property(x => x.Defenders).IsRequired();
            e.Property(x => x.Midfielders).IsRequired();
            e.Property(x => x.Attackers).IsRequired();
            e.HasIndex(x => x.Slug).IsUnique().HasFilter("\"Slug\" IS NOT NULL");
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        modelBuilder.Entity<Formation>(e =>
        {
            e.ToTable("Formations");
            e.Property(x => x.Code).IsRequired().HasMaxLength(20);
            e.HasIndex(x => x.Slug).IsUnique().HasFilter("\"Slug\" IS NOT NULL");
            e.HasIndex(x => x.FormationMarkId).IsUnique();
            e.HasOne(x => x.FormationMark)
                .WithOne(x => x.Formation)
                .HasForeignKey<Formation>(x => x.FormationMarkId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        modelBuilder.Entity<Position>(e =>
        {
            e.ToTable("Positions");
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(x => x.Slug).IsUnique().HasFilter("\"Slug\" IS NOT NULL");
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        modelBuilder.Entity<SubPosition>(e =>
        {
            e.ToTable("SubPositions");
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(x => x.Slug).IsUnique().HasFilter("\"Slug\" IS NOT NULL");
            e.HasOne(x => x.Position)
                .WithMany(x => x.SubPositions)
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        modelBuilder.Entity<Player>(e =>
        {
            e.HasOne(x => x.Position)
                .WithMany(x => x.Players)
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SubPosition)
                .WithMany(x => x.Players)
                .HasForeignKey(x => x.SubPositionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public DbSet<Country> Countries { get; set; }
    public DbSet<GeoData> GeoDatas { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Person> Persons { get; set; } = null!;
    public DbSet<Stadium> Stadiums { get; set; }
    public DbSet<MatchStatus> MatchStatuses { get; set; } = null!;
    public DbSet<Match> Matches { get; set; } = null!;
    public DbSet<FormationMark> FormationMarks { get; set; } = null!;
    public DbSet<Formation> Formations { get; set; } = null!;
    public DbSet<Position> Positions { get; set; } = null!;
    public DbSet<SubPosition> SubPositions { get; set; } = null!;
    public DbSet<PlayerSelectionHistory> PlayerSelectionHistories { get; set; } = null!;
    public DbSet<SelectionManagerHistory> SelectionManagerHistories { get; set; } = null!;
    public DbSet<TransferValueHistory> TransferValueHistories { get; set; } = null!;
    public DbSet<MandatDate> MandatDates { get; set; } = null!;
    public DbSet<Selection> Selections { get; set; } = null!;
    public DbSet<CompetitionEntity> Competitions { get; set; } = null!;
    public DbSet<Season> Seasons { get; set; } = null!;
    public DbSet<Round> Rounds { get; set; } = null!;

}