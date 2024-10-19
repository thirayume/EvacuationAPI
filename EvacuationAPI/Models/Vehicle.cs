using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EvacuationAPI.Models
{
    public class Vehicle
    {
        public required string VehicleID { get; set; }
        public required int Capacity { get; set; }
        public required string Type { get; set; }
        public required LocationCoordinates LocationCoordinates { get; set; }
        public required double Speed { get; set; }
    }
}
