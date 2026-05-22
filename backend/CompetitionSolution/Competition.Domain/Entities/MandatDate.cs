using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Domain.Entities
{
    public class MandatDate:BaseEntity
    {
        public LocalDate? StartMandatDate {  get; set; }
        public LocalDate? EndMandatDate { get; set; }
        public Guid? PlayerSelectionHistoryId {  get; set; }
        public PlayerSelectionHistory PlayerSelectionHistory { get; set; }
        public Guid? SelectionManagerHistoryId { get; set; }
        public SelectionManagerHistory SelectionManagerHistory { get; set; }
    }
}
