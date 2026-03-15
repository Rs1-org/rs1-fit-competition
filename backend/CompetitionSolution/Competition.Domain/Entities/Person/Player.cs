using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Domain.Entities.Person
{
    public class Player:BaseEntity
    {
        public decimal Height {  get; set; }
        public int ShirtNumber {  get; set; }
        public float TransferValue {  get; set; }
    }
}
