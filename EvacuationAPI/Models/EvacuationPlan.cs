namespace EvacuationAPI.Models
{
    public class EvacuationPlan
    {
        public required string ZoneID { get; set; }
        public required string VehicleID { get; set; }
        public required string ETA { get; set; }
        public int NumberOfPeople { get; set; }
    }
}
