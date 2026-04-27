using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Domain.Entities.Person
{
    public class Person:BaseEntity
    {
        //public int Id {  get; set; }
        public string FirstName {  get; set; }
        public string LastName { get; set; }
        public LocalTime DateOfBirth {  get; set; }
        public byte[] Picture {  get; set; }
    }
}
