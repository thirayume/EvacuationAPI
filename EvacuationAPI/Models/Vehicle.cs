using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EvacuationAPI.Models
{
    public class Vehicle
    {
        /// <summary>
        /// Unique identifier for each vehicle.
        /// </summary>
        public required string VehicleID { get; set; }
        /// <summary>
        /// Number of people the vehicle can transport in one trip.
        /// </summary>
        public required int Capacity { get; set; }
        /// <summary>
        /// Type of vehicle (e.g., bus, van, boat).
        /// </summary>
        public required string Type { get; set; }
        /// <summary>
        /// Latitude and longitude of the vehicle’s current location.
        /// </summary>
        public required LocationCoordinates LocationCoordinates { get; set; }
        /// <summary>
        /// Average speed of the vehicle in km/h.
        /// </summary>
        public required double Speed { get; set; }
    }
}
