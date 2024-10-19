namespace EvacuationAPI.Models
{
    public class LocationCoordinates
    {
        /// <summary>
        /// Geographic coordinates are angular units, latitude values are the y-coordinate
        /// </summary>
        public double Latitude { get; set; }
        /// <summary>
        /// Geographic coordinates are angular units, longitude values are considered the x-coordinate
        /// </summary>
        public double Longitude { get; set; }
    }
}
