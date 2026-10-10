using Competition.Domain.Entities.FormationEntities;

namespace Competition.Domain.Entities.Person
{
    public class Player:Person
    {
        public decimal Height {  get; set; }
        public int ShirtNumber {  get; set; }
        public float TransferValue {  get; set; }

        public Guid? PositionId { get; set; }
        public Position? Position { get; set; }

        public Guid? SubPositionId { get; set; }
        public SubPosition? SubPosition { get; set; }
    }
}
