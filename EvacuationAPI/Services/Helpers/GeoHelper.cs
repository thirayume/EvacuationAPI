using EvacuationAPI.Models;
using System.Runtime.Intrinsics.X86;

namespace EvacuationAPI.Services.Helpers
{
    public static class GeoHelper
    {
        private const double EarthRadiusKm = 6371.0; // Earth's radius in kilometers

        /// <summary>
        /// Distance Calculation using Haversine Formula: To accurately calculate the distance between coordinates(lat/long), use the Haversine formula.
        /// </summary>
        /// <param name="loc1"></param>
        /// <param name="loc2"></param>
        /// <returns></returns>
        public static double CalculateDistance(LocationCoordinates loc1, LocationCoordinates loc2)
        {
            var dLat = DegreesToRadians(loc2.Latitude - loc1.Latitude);
            var dLon = DegreesToRadians(loc2.Longitude - loc1.Longitude);

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(loc1.Latitude)) * Math.Cos(DegreesToRadians(loc2.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c; // Distance in kilometers
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        public static TimeSpan CalculateETA(double speed, double distance)
        {
            // Time = Distance / Speed; speed should be in the same units as distance (e.g., km/h if distance is in km)
            double timeInHours = distance / speed;

            // Convert hours into TimeSpan for more precise representation (hours and minutes)
            TimeSpan eta = TimeSpan.FromHours(timeInHours);

            return eta;
        }

    }
}
