namespace EvacuationAPI.Models
{
    public class EvacuationStatus
    {
        /// <summary>
        /// Zone ID
        /// </summary>
        public required string ZoneID { get; set; }

        /// <summary>
        /// Total number of people evacuated
        /// </summary>
        public int TotalEvacuated { get; set; }

        /// <summary>
        /// Remaining number of people needing evacuation
        /// </summary>
        public int RemainingPeople { get; set; }

        /// <summary>
        /// Last vehicle used for evacuation
        /// </summary>
        public string? LastVehicleUsed { get; set; } // Make it nullable
    }
}
