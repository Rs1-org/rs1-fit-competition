using Competition.Domain.Entities.Person;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Domain.Entities
{
    public class PlayerSelectionHistory:BaseEntity
    {
        public int? Goals {  get; set; }
        public int? MatchPlayed {  get; set; }
        public int? Assists {  get; set; }
        public int? ShirtNumber {  get; set; }
        public Guid PlayerId {  get; set; }
        public Player Player { get; set; }
    }
}
