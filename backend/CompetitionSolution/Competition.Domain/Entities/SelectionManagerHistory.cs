using Competition.Domain.Entities.Person;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Domain.Entities
{
    public class SelectionManagerHistory:BaseEntity
    {
        public string? Notes {  get; set; }
        public int? Wins { get; set; }
        public int? Loses {  get; set; }
        public int? Draw {  get; set; }
        public Guid ManagerId {  get; set; }
        public Manager Manager { get; set; }
    }
}
