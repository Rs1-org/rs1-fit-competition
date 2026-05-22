using Competition.Domain.Entities.Person;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Domain.Entities
{
    public class TransferValueHistory:BaseEntity
    {
        public LocalDate? Date { get; set; }
        public decimal? Value { get; set; }
        public Guid PlayerId { get; set; }
        public Player Player { get; set; }
    }
}
