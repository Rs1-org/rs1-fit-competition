using NodaTime;

namespace Competition.Domain.Entities;

public class MandateDate : BaseEntity
{
    public LocalDate? StartDate { get; set; }
    public LocalDate? EndDate { get; set; }
    public Guid? PlayerSelectionHistoryId { get; set; }
    public PlayerSelectionHistory? PlayerSelectionHistory { get; set; }
    public Guid? SelectionManagerHistoryId { get; set; }
    public SelectionManagerHistory? SelectionManagerHistory { get; set; }
}
