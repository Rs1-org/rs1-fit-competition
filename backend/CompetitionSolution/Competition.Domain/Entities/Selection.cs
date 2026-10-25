using Competition.Domain.Entities.MatchEntities;

namespace Competition.Domain.Entities;

public class Selection : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public string? PictureUrl { get; set; }
    public int FollowersCount { get; set; }
    public string? Slug { get; set; }

    // Nullable for legacy rows; create/update require an active club.
    public Guid? ClubId { get; set; }
    public Club? Club { get; set; }

    public Guid CountryId { get; set; }
    public Country? Country { get; set; }

    public Guid? HomeStadiumId { get; set; }
    public Stadium? HomeStadium { get; set; }

    public ICollection<PlayerSelectionHistory> PlayerHistories { get; set; } = [];
    public ICollection<SelectionManagerHistory> ManagerHistories { get; set; } = [];
    public ICollection<Match> HomeMatches { get; set; } = [];
    public ICollection<Match> AwayMatches { get; set; } = [];
}
