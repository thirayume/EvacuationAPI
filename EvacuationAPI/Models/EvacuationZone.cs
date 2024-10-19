using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EvacuationAPI.Models
{
    public class EvacuationZone
    {
        /// <summary>
        /// Unique identifier for the evacuation zone.
        /// </summary>
        public required string ZoneID { get; set; }
        /// <summary>
        /// Latitude and longitude of the zone.
        /// </summary>
        public required LocationCoordinates LocationCoordinates { get; set; }
        /// <summary>
        /// Total number of people needing evacuation.
        /// </summary>
        public required int NumberOfPeople { get; set; }
        /// <summary>
        /// Integer from 1 to 5 (1 = low urgency, 5 = high urgency).
        /// </summary>
        public required int UrgencyLevel { get; set; }
    }
}
