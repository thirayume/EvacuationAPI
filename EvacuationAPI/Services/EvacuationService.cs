using EvacuationAPI.Models;
using EvacuationAPI.Services.Helpers;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging; // Importing ILogger
using StackExchange.Redis;
using System.Numerics;
using System.Security.Policy;
using System.Text.Json;

namespace EvacuationAPI.Services
{
    public class EvacuationService(RedisDb redisDb, ILogger<EvacuationService> logger)
    {
        private readonly RedisDb _redisDb = redisDb;
        private readonly ILogger<EvacuationService> _logger = logger; // ILogger for logging

        public bool CheckRedisConnection()
        {
            try
            {
                // Get the database to trigger a connection check
                var db = _redisDb.Multiplexer.GetDatabase();

                // Ping the Redis server
                var result = db.Ping();

                // Optionally, log the result of the ping
                _logger.LogInformation("Redis connection is {Status}.", result.TotalMilliseconds > 0 ? "available" : "not available");

                return result.TotalMilliseconds > 0; // Return true if connection is successful
            }
            catch (Exception ex)
            {
                _logger.LogError("Error checking Redis connection: {Message}", ex.Message);
                return false; // Return false if there is an error
            }
        }

        public async Task<List<string>> GetKeysAsync()
        {
            try
            {
                var server = _redisDb.Multiplexer.GetServer(_redisDb.Multiplexer.GetEndPoints()[0]);
                var keys = new List<string>();

                await foreach (var key in server.KeysAsync())
                {
                    keys.Add(key.ToString());
                }

                return keys;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving Redis info: {Message}", ex.Message);
                throw; // Rethrow exception to handle in the controller
            }
        }

        #region CRUD operations for EvacuationZone

        public async Task<List<EvacuationZone>> GetAllEvacuationZonesAsync()
        {
            var zoneList = new List<EvacuationZone>();

            try
            {
                // Get the first endpoint (this should be a valid endpoint)
                var endPoint = _redisDb.Multiplexer.GetEndPoints()[0];
                var server = _redisDb.Multiplexer.GetServer(endPoint);

                // Fetch keys matching the pattern
                var keys = server.Keys(pattern: "evacuation-zone:*");

                // Process each key to retrieve the corresponding evacuation zone data
                foreach (var key in keys)
                {
                    var zoneData = await _redisDb.Multiplexer.GetDatabase().StringGetAsync(key);
                    if (!zoneData.IsNullOrEmpty)
                    {
                        // Deserialize the JSON data into an EvacuationZone object
                        var zone = JsonSerializer.Deserialize<EvacuationZone>(zoneData.ToString()!);
                        if (zone != null)
                        {
                            zoneList.Add(zone);
                            _logger.LogInformation("Successfully retrieved evacuation zone: {Id}", zone.ZoneID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError("Error retrieving evacuation zones: {Message}", ex.Message);
            }

            return zoneList;
        }

        public async Task<EvacuationZone?> GetEvacuationZoneByIdAsync(string id)
        {
            try
            {
                // Construct the key based on the ID
                var key = $"evacuation-zone:{id}";

                // Retrieve the zone data from Redis
                var zoneData = await _redisDb.Multiplexer.GetDatabase().StringGetAsync(key);

                // Check if the data is not null or empty
                if (!zoneData.IsNullOrEmpty)
                {
                    // Deserialize the JSON data into an EvacuationZone object
                    var zone = JsonSerializer.Deserialize<EvacuationZone>(zoneData.ToString()!);
                    return zone; // Return the found zone
                }
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError("Error retrieving evacuation zone by ID {Id}: {Message}", id, ex.Message);
                // Optionally, handle or rethrow the exception
            }

            return null; // Return null if no zone was found or an error occurred
        }

        public async Task AddOrUpdateEvacuationZonesAsync(List<EvacuationZone> zones)
        {
            foreach (var zone in zones)
            {
                try
                {
                    var zoneData = JsonSerializer.Serialize(zone);
                    await _redisDb.Multiplexer.GetDatabase().StringSetAsync($"evacuation-zone:{zone.ZoneID}", zoneData);
                    // Log successful operation
                    _logger.LogInformation("Zone {ZoneID} added/updated successfully.", zone.ZoneID);
                }
                catch (Exception ex)
                {
                    // Log the error
                    _logger.LogError("Error adding/updating zone {ZoneID}: {Message}", zone.ZoneID, ex.Message);
                }
            }
        }

        public async Task DeleteEvacuationZonesAsync(string[] zoneIDs)
        {
            if (zoneIDs == null || zoneIDs.Length == 0)
            {
                throw new ArgumentException("Zone IDs cannot be null or empty.", nameof(zoneIDs));
            }

            foreach (var id in zoneIDs)
            {
                // Assuming id is not null or empty; otherwise, you may want to validate it
                var key = $"evacuation-zone:{id}";
                bool wasDeleted = await _redisDb.Multiplexer.GetDatabase().KeyDeleteAsync(key);

                if (wasDeleted)
                {
                    // Log successful deletion
                    _logger.LogInformation("Successfully deleted zone: {id}", id);
                }
                else
                {
                    // Handle the case where the key did not exist
                    _logger.LogWarning("Zone not found: {id}", id);
                }
            }
        }

        #endregion

        #region CRUD operations for Vehicle

        public async Task<List<Vehicle>> GetAllVehiclesAsync()
        {
            var vehicleList = new List<Vehicle>();

            try
            {
                // Get the first endpoint (this should be a valid endpoint)
                var endPoint = _redisDb.Multiplexer.GetEndPoints()[0];
                var server = _redisDb.Multiplexer.GetServer(endPoint);

                // Fetch keys matching the pattern
                var keys = server.Keys(pattern: "vehicle:*");

                // Process each key to retrieve the corresponding vehicle data
                foreach (var key in keys)
                {
                    var vehicleData = await _redisDb.Multiplexer.GetDatabase().StringGetAsync(key);
                    if (!vehicleData.IsNullOrEmpty)
                    {
                        // Deserialize the JSON data into an Vehicle object
                        var vehicle = JsonSerializer.Deserialize<Vehicle>(vehicleData.ToString()!);
                        if (vehicle != null)
                        {
                            vehicleList.Add(vehicle);
                            _logger.LogInformation("Successfully retrieved vehicle: {Id}", vehicle.VehicleID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError("Error retrieving vehicles: {Message}", ex.Message);
            }

            return vehicleList;
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(string id)
        {
            try
            {
                // Construct the key based on the ID
                var key = $"vehicle:{id}";

                // Retrieve the vehicle data from Redis
                var vehicleData = await _redisDb.Multiplexer.GetDatabase().StringGetAsync(key);

                // Check if the data is not null or empty
                if (!vehicleData.IsNullOrEmpty)
                {
                    // Deserialize the JSON data into a Vehicle object
                    var vehicle = JsonSerializer.Deserialize<Vehicle>(vehicleData.ToString()!);
                    return vehicle; // Return the found vehicle
                }
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError("Error retrieving vehicle by ID {Id}: {Message}", id, ex.Message);
                // Optionally, handle or rethrow the exception
            }

            return null; // Return null if no vehicle was found or an error occurred
        }

        public async Task AddOrUpdateVehiclesAsync(List<Vehicle> vehicles)
        {
            foreach (var vehicle in vehicles)
            {
                var vehicleData = JsonSerializer.Serialize(vehicle);
                await _redisDb.Multiplexer.GetDatabase().StringSetAsync($"vehicle:{vehicle.VehicleID}", vehicleData);
                _logger.LogInformation("Vehicle {VehicleID} added/updated successfully.", vehicle.VehicleID); // Log successful operation
            }
        }

        public async Task DeleteVehiclesAsync(string[] zoneIDs)
        {
            foreach (var id in zoneIDs)
            {
                await _redisDb.Multiplexer.GetDatabase().KeyDeleteAsync($"vehicle:{id}");
                _logger.LogInformation("Vehicle {id} deleted successfully.", id); // Log successful deletion
            }
        }

        #endregion

        // Get evacuation status
        public async Task<List<EvacuationStatus>> GetEvacuationStatusAsync()
        {
            var existingStatusData = await _redisDb.Multiplexer.GetDatabase().StringGetAsync("evacuation-statuses");

            // Deserialize existing statuses
            var existingStatuses = string.IsNullOrEmpty(existingStatusData)
                ? []
                : JsonSerializer.Deserialize<List<EvacuationStatus>>(existingStatusData.ToString()!) ?? [];

            // Filter to get the status for each zone with the minimum RemainingPeople
            var finalStatuses = existingStatuses
                .GroupBy(s => s.ZoneID)
                .Select(g => g.OrderBy(s => s.RemainingPeople).FirstOrDefault())
                .Where(s => s != null) // Ensure no nulls are returned
                .Cast<EvacuationStatus>() // Cast to EvacuationStatus to remove the nullable reference type
                .ToList();

            return finalStatuses; // Now this is List<EvacuationStatus>
        }


        // Clear evacuation plans
        public async Task ClearEvacuationPlansAsync()
        {
            // Clean up existing plans before saving new ones
            await _redisDb.Multiplexer.GetDatabase().KeyDeleteAsync("evacuation-plans"); // Delete existing plans
            // Clean up existing plans before saving new ones
            await _redisDb.Multiplexer.GetDatabase().KeyDeleteAsync("evacuation-statuses"); // Delete existing statuses
        }

        // Generate evacuation plan
        public async Task SaveEvacuationPlanAsync(List<EvacuationPlan> plans)
        {
            // Clean up existing plans before saving new ones
            await _redisDb.Multiplexer.GetDatabase().KeyDeleteAsync("evacuation-plans"); // Delete existing plans
            var planData = JsonSerializer.Serialize(plans);
            await _redisDb.Multiplexer.GetDatabase().StringSetAsync("evacuation-plans", planData);
        }

        public async Task LogEvacuationStatusesAsync(List<EvacuationStatus> statuses)
        {
            // Clean up existing plans before saving new ones
            await _redisDb.Multiplexer.GetDatabase().KeyDeleteAsync("evacuation-statuses"); // Delete existing statuses
            var statusData = JsonSerializer.Serialize(statuses);
            await _redisDb.Multiplexer.GetDatabase().StringSetAsync("evacuation-statuses", statusData);
        }

        public async Task AppendEvacuationStatusesAsync(List<EvacuationStatus> newStatuses)
        {
            // Retrieve existing evacuation statuses from Redis
            var existingStatusData = await _redisDb.Multiplexer.GetDatabase().StringGetAsync("evacuation-statuses");

            // Deserialize existing statuses
            var existingStatuses = string.IsNullOrEmpty(existingStatusData)
                ? []
                : JsonSerializer.Deserialize<List<EvacuationStatus>>(existingStatusData.ToString()!) ?? [];

            // Check for duplicates and append new statuses
            foreach (var newStatus in newStatuses)
            {
                // Check if the status already exists based on ZoneID and TotalEvacuated
                if (!existingStatuses.Any(s => s.ZoneID == newStatus.ZoneID && s.TotalEvacuated == newStatus.TotalEvacuated))
                {
                    existingStatuses.Add(newStatus); // Append if not duplicate
                }
            }

            // Serialize the updated list and save it back to Redis
            var updatedStatusData = JsonSerializer.Serialize(existingStatuses);
            await _redisDb.Multiplexer.GetDatabase().StringSetAsync("evacuation-statuses", updatedStatusData);
        }


        public async Task<List<EvacuationPlan>> AutoGenerateEvacuationPlanAsync()
        {
            var zones = await GetAllEvacuationZonesAsync();
            var vehicles = await GetAllVehiclesAsync();

            if (zones == null || vehicles == null)
            {
                throw new Exception("Evacuation zones or vehicles not available.");
            }

            // Sort zones by urgency level (higher urgency first)
            zones = [.. zones.OrderByDescending(z => z.UrgencyLevel)];
            var evacuationPlans = new List<EvacuationPlan>();
            var evacuationStatuses = new List<EvacuationStatus>();

            // Pre-calculate distances and ETAs
            var vehicleDistances = new Dictionary<string, Dictionary<string, double>>();
            var vehicleETAs = new Dictionary<string, Dictionary<string, TimeSpan>>();

            foreach (var vehicle in vehicles)
            {
                vehicleDistances[vehicle.VehicleID] = [];
                vehicleETAs[vehicle.VehicleID] = [];

                foreach (var zone in zones)
                {
                    double distance = GeoHelper.CalculateDistance(vehicle.LocationCoordinates, zone.LocationCoordinates);
                    TimeSpan eta = GeoHelper.CalculateETA(vehicle.Speed, distance);
                    vehicleDistances[vehicle.VehicleID][zone.ZoneID] = distance;
                    vehicleETAs[vehicle.VehicleID][zone.ZoneID] = eta;
                }
            }

            List<Vehicle> usedVehicles = [];

            // Process each zone until all have been evacuated
            foreach (var zone in zones)
            {
                // Create a temporary list of available vehicles for this zone
                var availableVehicles = vehicles
                    .Where(v => v.Capacity > 0) // Check if the vehicle has capacity
                    .OrderByDescending(v => v.Capacity) // Sort by capacity (highest first)
                    .ThenBy(v => vehicleDistances[v.VehicleID][zone.ZoneID]) // Sort by distance (shortest first)
                    .Except(usedVehicles) // Remove used vehicles
                    .ToList(); // Get the sorted list of available vehicles

                // Find the best match vehicle for this zone
                var bestVehicle = availableVehicles.FirstOrDefault(); // Get the best match vehicle

                if (bestVehicle == null)
                {
                    _logger.LogWarning("No available vehicles for zone {ZoneID}. Skipping this zone.", zone.ZoneID);
                    break; // Skip this zone and continue to the next one
                }

                // Calculate the ETA for the current round
                var eta = vehicleETAs[bestVehicle.VehicleID][zone.ZoneID]; // Use pre-calculated ETA

                // Create the evacuation plan
                var evacuationPlan = new EvacuationPlan
                {
                    ZoneID = zone.ZoneID,
                    VehicleID = bestVehicle.VehicleID,
                    NumberOfPeople = zone.NumberOfPeople,
                    ETA = $"{eta.Hours}h {eta.Minutes}m per round"
                };

                evacuationPlans.Add(evacuationPlan);

                int remainingPeople = zone.NumberOfPeople;

                // Skip zones that have already been fully evacuated
                while (remainingPeople > 0)
                {
                    // Log progress
                    _logger.LogInformation("Processing zone {ZoneID}: {NumberOfPeople} people remaining...", zone.ZoneID, zone.NumberOfPeople);

                    // Calculate how many evacuees can be transported in this round
                    int evacueesToTransport = Math.Min(remainingPeople, bestVehicle.Capacity);
                    remainingPeople -= evacueesToTransport; // Decrement the number of people in the zone
                    // Ensure RemainingPeople does not go below zero
                    remainingPeople = Math.Max(0, remainingPeople); // Avoid negative numbers

                    // Create and log the evacuation status
                    var evacuationStatus = new EvacuationStatus
                    {
                        ZoneID = zone.ZoneID,
                        TotalEvacuated = (zone.NumberOfPeople - remainingPeople),
                        RemainingPeople = remainingPeople,
                        LastVehicleUsed = bestVehicle.VehicleID
                    };

                    evacuationStatuses.Add(evacuationStatus);

                    // Log evacuation action
                    _logger.LogInformation("Evacuated {evacueesToTransport} people from zone {ZoneID} using vehicle {VehicleID}. Remaining: {remainingPeople}.", evacueesToTransport, zone.ZoneID, bestVehicle.VehicleID, remainingPeople);

                    // If the zone is fully evacuated, break out of the inner loop
                    if (remainingPeople <= 0)
                    {
                        _logger.LogInformation("Zone {ZoneID} has been fully evacuated.", zone.ZoneID);
                        break;
                    }
                }

                usedVehicles.Add(bestVehicle);
            }

            // Save the evacuation plans to Redis
            await SaveEvacuationPlanAsync(evacuationPlans);

            // Log evacuation statuses to Redis
            await LogEvacuationStatusesAsync(evacuationStatuses);

            return evacuationPlans;
        }

        public async Task<List<EvacuationPlan>> PlanEvacuationAsync()
        {
            var zones = await GetAllEvacuationZonesAsync();
            var vehicles = await GetAllVehiclesAsync();

            if (zones == null || vehicles == null)
            {
                throw new Exception("Evacuation zones or vehicles not available.");
            }

            // Sort zones by urgency level (higher urgency first)
            zones = [.. zones.OrderByDescending(z => z.UrgencyLevel)];
            var evacuationPlans = new List<EvacuationPlan>();
            var evacuationStatuses = new List<EvacuationStatus>();

            // Pre-calculate distances and ETAs
            var vehicleDistances = new Dictionary<string, Dictionary<string, double>>();
            var vehicleETAs = new Dictionary<string, Dictionary<string, TimeSpan>>();

            foreach (var vehicle in vehicles)
            {
                vehicleDistances[vehicle.VehicleID] = [];
                vehicleETAs[vehicle.VehicleID] = [];

                foreach (var zone in zones)
                {
                    double distance = GeoHelper.CalculateDistance(vehicle.LocationCoordinates, zone.LocationCoordinates);
                    TimeSpan eta = GeoHelper.CalculateETA(vehicle.Speed, distance);
                    vehicleDistances[vehicle.VehicleID][zone.ZoneID] = distance;
                    vehicleETAs[vehicle.VehicleID][zone.ZoneID] = eta;
                }
            }

            List<Vehicle> usedVehicles = [];

            // Process each zone until all have been evacuated
            foreach (var zone in zones)
            {
                // Create a temporary list of available vehicles for this zone
                var availableVehicles = vehicles
                    .Where(v => v.Capacity > 0) // Check if the vehicle has capacity
                    .OrderByDescending(v => v.Capacity) // Sort by capacity (highest first)
                    .ThenBy(v => vehicleDistances[v.VehicleID][zone.ZoneID]) // Sort by distance (shortest first)
                    .Except(usedVehicles) // Remove used vehicles
                    .ToList(); // Get the sorted list of available vehicles

                // Find the best match vehicle for this zone
                var bestVehicle = availableVehicles.FirstOrDefault(); // Get the best match vehicle

                if (bestVehicle == null)
                {
                    _logger.LogWarning("No available vehicles for zone {ZoneID}. Skipping this zone.", zone.ZoneID);
                    continue; // Skip this zone and continue to the next one
                }

                // Create the evacuation plan
                var evacuationPlan = new EvacuationPlan
                {
                    ZoneID = zone.ZoneID,
                    VehicleID = bestVehicle.VehicleID,
                    NumberOfPeople = zone.NumberOfPeople,
                    ETA = $"{vehicleETAs[bestVehicle.VehicleID][zone.ZoneID].Hours}h {vehicleETAs[bestVehicle.VehicleID][zone.ZoneID].Minutes}m per round"
                };

                evacuationPlans.Add(evacuationPlan);

                // Create the evacuation status
                var evacuationStatus = new EvacuationStatus
                {
                    ZoneID = zone.ZoneID,
                    TotalEvacuated = 0, // Calculate the total evacuated
                    RemainingPeople = zone.NumberOfPeople, // Update remaining people
                    LastVehicleUsed = bestVehicle.VehicleID // Use the vehicle from the plan
                };

                evacuationStatuses.Add(evacuationStatus);

                // Log the evacuation action
                _logger.LogInformation("Evacuated {evacueesToTransport} people from zone {zoneID} using vehicle {VehicleID}. Remaining: {evacuees}.", 0, zone.ZoneID, bestVehicle.VehicleID, zone.NumberOfPeople);

                usedVehicles.Add(bestVehicle);
            }

            // Save the evacuation plans to Redis
            await SaveEvacuationPlanAsync(evacuationPlans);

            // Log evacuation status to Redis
            await LogEvacuationStatusesAsync(evacuationStatuses);

            return evacuationPlans;
        }

        public async Task<List<EvacuationPlan>> GetEvacuationPlansAsync()
        {
            var existingPlansData = await _redisDb.Multiplexer.GetDatabase().StringGetAsync("evacuation-plans");

            // Deserialize existing evacuation plans
            var existingPlans = string.IsNullOrEmpty(existingPlansData)
                ? []
                : JsonSerializer.Deserialize<List<EvacuationPlan>>(existingPlansData.ToString()!) ?? [];

            return existingPlans;
        }

        public async Task<EvacuationStatus> UpdateEvacuationAsync(string zoneID, int evacuees)
        {
            // Fetch the evacuation plans
            var evacuationPlans = await GetEvacuationPlansAsync(); // Retrieve the stored evacuation plans from Redis
            var plan = evacuationPlans.FirstOrDefault(p => p.ZoneID == zoneID) ?? throw new Exception($"No evacuation plan found for zone {zoneID}.");

            // Get the vehicle details
            var vehicle = await GetVehicleByIdAsync(plan.VehicleID) ?? throw new Exception($"No vehicle found with ID {plan.VehicleID}.");

            // Get all evacuation statuses
            var allStatuses = await GetEvacuationStatusAsync();

            // Filter by the provided ZoneID
            var filteredStatuses = allStatuses
                .Where(status => status.ZoneID.Equals(zoneID, StringComparison.OrdinalIgnoreCase))
                .First();

            // Calculate how many evacuees can be transported in this round based on vehicle capacity
            int evacueesToTransport = Math.Min(evacuees, vehicle.Capacity);
            filteredStatuses.RemainingPeople -= evacueesToTransport; // Decrement the number of evacuees remaining
            // Ensure RemainingPeople does not go below zero
            if (filteredStatuses.RemainingPeople < 0)
            {
                evacueesToTransport += filteredStatuses.RemainingPeople;
            }            
            filteredStatuses.RemainingPeople = Math.Max(0, filteredStatuses.RemainingPeople); // Avoid negative numbers

            // Create the evacuation status
            var evacuationStatus = new EvacuationStatus
            {
                ZoneID = zoneID,
                TotalEvacuated = (evacueesToTransport + filteredStatuses.TotalEvacuated), // Calculate the total evacuated
                RemainingPeople = filteredStatuses.RemainingPeople, // Update remaining people
                LastVehicleUsed = plan.VehicleID // Use the vehicle from the plan
            };

            // Log the evacuation action
            _logger.LogInformation("Evacuated {evacueesToTransport} people from zone {zoneID} using vehicle {VehicleID}. Remaining: {evacuees}.", evacueesToTransport, zoneID, plan.VehicleID, evacuees);

            // Log evacuation status to Redis
            await AppendEvacuationStatusesAsync([evacuationStatus]);

            // Return the evacuation status
            return evacuationStatus;
        }
    }
}