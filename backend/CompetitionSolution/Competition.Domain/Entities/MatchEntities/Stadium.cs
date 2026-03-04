using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Domain.Entities.MatchEntities
{
    public class Stadium : BaseEntity
    {
        public string? Name { get; set; }
        public int Capacity { get; set; }
        public DateTime DateOfConstruction { get; set; }
        public string? Slug { get; set; }
        public Guid GeoDataId { get; set; }
        public GeoData? GeoData { get; set; }
        //public List<Selection>? Selections { get; set; }
        public string? PictureUrl { get; set; }
        public Guid CityId { get; set; }
        public City? City { get; set; }
    }
}
