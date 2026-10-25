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

        foreach (var entry in ChangeTracker.Entries<ApplicationUser>().Where(x => x.State == EntityState.Added))
            entry.Entity.RegisteredAt ??= now;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.ModifiedAt = now;
                if (string.IsNullOrEmpty(entry.Entity.PublicId))
                    entry.Entity.PublicId = BaseEntity.NewPublicId();
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

        modelBuilder.Entity<ApplicationUser>(e =>
        {
            e.Property(x => x.PublicId).IsRequired().HasMaxLength(BaseEntity.PublicIdLength)
                .HasDefaultValueSql("replace(gen_random_uuid()::text, '-', '')");
            e.HasIndex(x => x.PublicId).IsUnique();
        });

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

                // PublicId lives on the root table only (Player/Manager/Referee share Persons via TPT).
                if (entityType.BaseType is null)
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(nameof(BaseEntity.PublicId))
                        .IsRequired()
                        .HasMaxLength(BaseEntity.PublicIdLength);
                    modelBuilder.Entity(entityType.ClrType)
                        .HasIndex(nameof(BaseEntity.PublicId))
                        .IsUnique();
                }
            }
        }

        modelBuilder.Entity<GeoData>(e =>
        {
            e.ToTable("GeoDatas");
            e.Property(x => x.Latitude).HasPrecision(9, 6);
            e.Property(x => x.Longitude).HasPrecision(9, 6);
            e.Property(x => x.Description).HasMaxLength(1000);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        modelBuilder.Entity<Country>(e =>
        {
            e.ToTable("Countries");
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Slug).IsRequired().HasMaxLength(120);
            e.Property(x => x.PictureUrl).HasMaxLength(500);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        modelBuilder.Entity<City>(e =>
        {
            e.ToTable("Cities");
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Slug).IsRequired().HasMaxLength(120);
            e.HasOne(x => x.Country)
                .WithMany(c => c.Cities)
                .HasForeignKey(x => x.CountryId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.GeoData)
                .WithMany()
                .HasForeignKey(x => x.GeoDataId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        modelBuilder.Entity<Stadium>(e =>
        {
            e.ToTable("Stadiums");
            e.Property(x => x.Name).IsRequired().HasMaxLength(150);
            e.Property(x => x.Slug).IsRequired().HasMaxLength(170);
            e.Property(x => x.PictureUrl).HasMaxLength(500);
            e.HasOne(x => x.City)
                .WithMany()
                .HasForeignKey(x => x.CityId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.GeoData)
                .WithMany()
                .HasForeignKey(x => x.GeoDataId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

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

        modelBuilder.Entity<Poll>(e =>
        {
            e.ToTable("Polls", t => t.HasCheckConstraint("CK_Polls_TimeWindow", "\"OpensAt\" < \"ClosesAt\""));
            e.Property(x => x.Question).IsRequired().HasMaxLength(200);
            e.HasOne(x => x.Match).WithMany().HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.DeletedAt == null && x.Match.DeletedAt == null);
        });
        modelBuilder.Entity<PollOption>(e =>
        {
            e.ToTable("PollOptions");
            e.Property(x => x.Label).IsRequired().HasMaxLength(100);
            e.HasOne(x => x.Poll).WithMany(x => x.Options).HasForeignKey(x => x.PollId).OnDelete(DeleteBehavior.Restrict);
            e.HasAlternateKey(x => new { x.PollId, x.Id });
            e.HasIndex(x => new { x.PollId, x.SortOrder }).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            e.HasQueryFilter(x => x.DeletedAt == null && x.Poll.DeletedAt == null && x.Poll.Match.DeletedAt == null);
        });
        modelBuilder.Entity<PollVote>(e =>
        {
            e.ToTable("PollVotes");
            e.HasOne(x => x.Poll).WithMany(x => x.Votes).HasForeignKey(x => x.PollId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Option).WithMany(x => x.Votes).HasForeignKey(x => new { x.PollId, x.OptionId })
                .HasPrincipalKey(x => new { x.PollId, x.Id }).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.PollId, x.UserId }).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            e.HasQueryFilter(x => x.DeletedAt == null && x.Poll.DeletedAt == null && x.Poll.Match.DeletedAt == null && x.Option.DeletedAt == null);
        });

        modelBuilder.Entity<EventType>(e =>
        {
            e.ToTable("EventTypes");
            e.Property(x => x.Code).IsRequired().HasMaxLength(40);
            e.Property(x => x.Name).IsRequired().HasMaxLength(80);
            e.HasIndex(x => x.Code).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            e.HasQueryFilter(x => x.DeletedAt == null);
        });
        modelBuilder.Entity<MatchPart>(e =>
        {
            e.ToTable("MatchParts");
            e.Property(x => x.Code).IsRequired().HasMaxLength(40);
            e.Property(x => x.Name).IsRequired().HasMaxLength(80);
            e.HasIndex(x => x.Code).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            e.HasQueryFilter(x => x.DeletedAt == null);
        });
        modelBuilder.Entity<Event>(e =>
        {
            e.ToTable("Events", t =>
            {
                t.HasCheckConstraint("CK_Events_Minute", "\"Minute\" BETWEEN 0 AND 130 AND \"AddedMinute\" BETWEEN 0 AND 30");
                t.HasCheckConstraint("CK_Events_DifferentPlayers", "\"PlayerId\" IS NULL OR \"RelatedPlayerId\" IS NULL OR \"PlayerId\" <> \"RelatedPlayerId\"");
            });
            e.Property(x => x.Note).HasMaxLength(500);
            e.HasOne(x => x.Match).WithMany().HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.EventType).WithMany().HasForeignKey(x => x.EventTypeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.MatchPart).WithMany().HasForeignKey(x => x.MatchPartId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Selection).WithMany().HasForeignKey(x => x.SelectionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Player).WithMany().HasForeignKey(x => x.PlayerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.RelatedPlayer).WithMany().HasForeignKey(x => x.RelatedPlayerId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.MatchId, x.MatchPartId, x.Minute, x.AddedMinute });
            e.HasQueryFilter(x => x.DeletedAt == null && x.Match.DeletedAt == null
                && x.EventType.DeletedAt == null && x.MatchPart.DeletedAt == null && x.Selection.DeletedAt == null);
        });

        modelBuilder.Entity<MatchReview>(e =>
        {
            e.ToTable("MatchReviews", table => table.HasCheckConstraint("CK_MatchReviews_Rating", "\"Rating\" BETWEEN 1 AND 5"));
            e.Property(x => x.Comment).IsRequired().HasMaxLength(2000);
            e.HasOne(x => x.Match).WithMany().HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Author).WithMany().HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.MatchId, x.AuthorId }).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            e.HasQueryFilter(x => x.DeletedAt == null && x.Match.DeletedAt == null);
        });

        modelBuilder.Entity<ReviewReaction>(e =>
        {
            e.ToTable("ReviewReactions", table => table.HasCheckConstraint("CK_ReviewReactions_Value", "\"Value\" IN (-1, 1)"));
            e.HasOne(x => x.Review).WithMany(x => x.Reactions).HasForeignKey(x => x.ReviewId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.ReviewId, x.UserId }).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            e.HasQueryFilter(x => x.DeletedAt == null && x.Review.DeletedAt == null && x.Review.Match.DeletedAt == null);
        });

        modelBuilder.Entity<Statistic>(e =>
        {
            e.ToTable("Statistics");
            e.HasOne(x => x.Match).WithMany(x => x.Statistics)
                .HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.MatchId).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            e.HasQueryFilter(x => x.DeletedAt == null && x.Match.DeletedAt == null);
        });

        modelBuilder.Entity<TeamStatistic>(e =>
        {
            e.ToTable("TeamStatistics", table =>
            {
                table.HasCheckConstraint("CK_TeamStatistics_NonNegative",
                    "\"TotalShots\" >= 0 AND \"ShotsOnTarget\" >= 0 AND \"TotalRedCards\" >= 0 AND \"TotalYellowCards\" >= 0 AND \"Corners\" >= 0 AND \"BigChances\" >= 0");
                table.HasCheckConstraint("CK_TeamStatistics_ShotsOnTarget", "\"ShotsOnTarget\" <= \"TotalShots\"");
                table.HasCheckConstraint("CK_TeamStatistics_Possession", "\"Possession\" >= 0 AND \"Possession\" <= 100");
            });
            e.HasOne(x => x.Statistic).WithMany(x => x.TeamStatistics)
                .HasForeignKey(x => x.StatisticId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Selection).WithMany()
                .HasForeignKey(x => x.SelectionId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.StatisticId, x.SelectionId }).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            e.HasQueryFilter(x => x.DeletedAt == null && x.Statistic.DeletedAt == null
                && x.Statistic.Match.DeletedAt == null && x.Selection.DeletedAt == null);
        });

        modelBuilder.Entity<Club>(e =>
        {
            e.ToTable("Clubs");
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Slug).IsRequired().HasMaxLength(120);
            e.HasIndex(x => x.Slug).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            e.HasOne(x => x.City).WithMany().HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Stadium).WithMany().HasForeignKey(x => x.StadiumId).OnDelete(DeleteBehavior.Restrict);
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
            e.HasOne(x => x.Club).WithMany().HasForeignKey(x => x.ClubId).OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.Name).IsRequired().HasMaxLength(120);
            e.Property(x => x.Abbreviation).IsRequired().HasMaxLength(10);
            e.HasIndex(x => x.Slug).IsUnique().HasFilter("\"Slug\" IS NOT NULL");
            e.HasOne(x => x.Country)
                .WithMany()
                .HasForeignKey(x => x.CountryId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.HomeStadium)
                .WithMany()
                .HasForeignKey(x => x.HomeStadiumId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        modelBuilder.Entity<PlayerSelectionHistory>(e =>
        {
            e.ToTable("PlayerSelectionHistories");
            e.HasOne(x => x.Player)
                .WithMany()
                .HasForeignKey(x => x.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Selection)
                .WithMany(s => s.PlayerHistories)
                .HasForeignKey(x => x.SelectionId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        modelBuilder.Entity<SelectionManagerHistory>(e =>
        {
            e.ToTable("SelectionManagerHistories");
            e.Property(x => x.Notes).HasMaxLength(2000);
            e.HasOne(x => x.Manager)
                .WithMany()
                .HasForeignKey(x => x.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Selection)
                .WithMany(s => s.ManagerHistories)
                .HasForeignKey(x => x.SelectionId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        modelBuilder.Entity<TransferValueHistory>(e =>
        {
            e.ToTable("TransferValueHistories");
            e.Property(x => x.Value).HasPrecision(18, 2);
            e.HasOne(x => x.Player)
                .WithMany()
                .HasForeignKey(x => x.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        modelBuilder.Entity<MandateDate>(e =>
        {
            e.ToTable("MandateDates");
            e.HasOne(x => x.PlayerSelectionHistory)
                .WithMany()
                .HasForeignKey(x => x.PlayerSelectionHistoryId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SelectionManagerHistory)
                .WithMany()
                .HasForeignKey(x => x.SelectionManagerHistoryId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.DeletedAt == null);
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

        modelBuilder.Entity<UserPreference>(e =>
        {
            e.ToTable("UserPreferences");
            e.HasKey(x => x.UserId);
            e.Property(x => x.Theme).IsRequired().HasMaxLength(10);
            e.Property(x => x.Language).IsRequired().HasMaxLength(5);
            e.Property(x => x.DateFormat).IsRequired().HasMaxLength(20);
            e.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<UserPreference>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public DbSet<Country> Countries { get; set; }
    public DbSet<GeoData> GeoDatas { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Person> Persons { get; set; } = null!;
    public DbSet<Stadium> Stadiums { get; set; }
    public DbSet<MatchStatus> MatchStatuses { get; set; } = null!;
    public DbSet<Match> Matches { get; set; } = null!;
    public DbSet<Poll> Polls { get; set; } = null!;
    public DbSet<PollOption> PollOptions { get; set; } = null!;
    public DbSet<PollVote> PollVotes { get; set; } = null!;
    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<EventType> EventTypes { get; set; } = null!;
    public DbSet<MatchPart> MatchParts { get; set; } = null!;
    public DbSet<Statistic> Statistics { get; set; } = null!;
    public DbSet<MatchReview> MatchReviews { get; set; } = null!;
    public DbSet<ReviewReaction> ReviewReactions { get; set; } = null!;
    public DbSet<TeamStatistic> TeamStatistics { get; set; } = null!;
    public DbSet<FormationMark> FormationMarks { get; set; } = null!;
    public DbSet<Formation> Formations { get; set; } = null!;
    public DbSet<Position> Positions { get; set; } = null!;
    public DbSet<SubPosition> SubPositions { get; set; } = null!;
    public DbSet<PlayerSelectionHistory> PlayerSelectionHistories { get; set; } = null!;
    public DbSet<SelectionManagerHistory> SelectionManagerHistories { get; set; } = null!;
    public DbSet<TransferValueHistory> TransferValueHistories { get; set; } = null!;
    public DbSet<MandateDate> MandateDates { get; set; } = null!;
    public DbSet<Selection> Selections { get; set; } = null!;
    public DbSet<Club> Clubs { get; set; } = null!;
    public DbSet<CompetitionEntity> Competitions { get; set; } = null!;
    public DbSet<Season> Seasons { get; set; } = null!;
    public DbSet<Round> Rounds { get; set; } = null!;
    public DbSet<UserPreference> UserPreferences { get; set; } = null!;

}
