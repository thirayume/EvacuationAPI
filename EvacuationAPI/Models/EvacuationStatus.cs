namespace EvacuationAPI.Models
{
    public class EvacuationStatus
    {
        public required string ZoneID { get; set; }
        public int TotalEvacuated { get; set; }
        public int RemainingPeople { get; set; }
        public string? LastVehicleUsed { get; set; }
    }
}
