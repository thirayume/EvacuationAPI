namespace EvacuationAPI.Models
{
    public class EvacuationPlan
    {
        /// <summary>
        /// Zone
        /// </summary>
        public required string ZoneID { get; set; }
        /// <summary>
        /// Vehicle
        /// </summary>
        public required string VehicleID { get; set; }
        /// <summary>
        /// Estimated Time of Arrival
        /// </summary>
        public required string ETA { get; set; }
        /// <summary>
        ///  Number of People to be Evacuated
        /// </summary>
        public int NumberOfPeople { get; set; }
    }
}
