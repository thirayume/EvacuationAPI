using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EvacuationAPI.Models
{
    public class EvacuationZone
    {
        public required string ZoneID { get; set; }
        public required LocationCoordinates LocationCoordinates { get; set; }
        public required int NumberOfPeople { get; set; }
        public required int UrgencyLevel { get; set; }
    }
}
