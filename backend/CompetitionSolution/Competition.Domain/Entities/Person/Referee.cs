using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Domain.Entities.Person
{
    public class Referee:Person
    {
        public LocalTime? CareerStart { get; set; }
    }
}
