using EvacuationAPI.Models;
using EvacuationAPI.Services.Helpers;
using System.Text.Json;

namespace EvacuationAPI.Services
{
    public class EvacuationService(RedisDb redisDb, ILogger<EvacuationService> logger)
    {
        private readonly RedisDb _redisDb = redisDb;
        private readonly ILogger<EvacuationService> _logger = logger;

        public bool CheckRedisConnection()
        {
            try
            {
                var db = _redisDb.Multiplexer.GetDatabase();
                var result = db.Ping();
                _logger.LogInformation("Redis connection is {Status}.", result.TotalMilliseconds > 0 ? "available" : "not available");
                return result.TotalMilliseconds > 0;
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
                throw;
            }
        }

        #region CRUD operations for EvacuationZone

        public async Task<List<EvacuationZone>> GetAllEvacuationZonesAsync()
        {
            var zoneList = new List<EvacuationZone>();

            try
            {
                var endPoint = _redisDb.Multiplexer.GetEndPoints()[0];
                var server = _redisDb.Multiplexer.GetServer(endPoint);
                var keys = server.Keys(pattern: "evacuation-zone:*");
                
                foreach (var key in keys)
                {
                    var zoneData = await _redisDb.Multiplexer.GetDatabase().StringGetAsync(key);
                    if (!zoneData.IsNullOrEmpty)
                    {
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
                _logger.LogError("Error retrieving evacuation zones: {Message}", ex.Message);
            }

            return zoneList;
        }

        public async Task<EvacuationZone?> GetEvacuationZoneByIdAsync(string id)
        {
            try
            {
                var key = $"evacuation-zone:{id}";
                var zoneData = await _redisDb.Multiplexer.GetDatabase().StringGetAsync(key);

                if (!zoneData.IsNullOrEmpty)
                {
                    var zone = JsonSerializer.Deserialize<EvacuationZone>(zoneData.ToString()!);
                    return zone;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving evacuation zone by ID {Id}: {Message}", id, ex.Message);
            }

            return null;
        }

        public async Task AddOrUpdateEvacuationZonesAsync(List<EvacuationZone> zones)
        {
            foreach (var zone in zones)
            {
                try
                {
                    var zoneData = JsonSerializer.Serialize(zone);
                    await _redisDb.Multiplexer.GetDatabase().StringSetAsync($"evacuation-zone:{zone.ZoneID}", zoneData);
                    _logger.LogInformation("Zone {ZoneID} added/updated successfully.", zone.ZoneID);
                }
                catch (Exception ex)
                {
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
                var key = $"evacuation-zone:{id}";
                bool wasDeleted = await _redisDb.Multiplexer.GetDatabase().KeyDeleteAsync(key);

                if (wasDeleted)
                {
                    _logger.LogInformation("Successfully deleted zone: {id}", id);
                }
                else
                {
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
                var endPoint = _redisDb.Multiplexer.GetEndPoints()[0];
                var server = _redisDb.Multiplexer.GetServer(endPoint);
                var keys = server.Keys(pattern: "vehicle:*");

                foreach (var key in keys)
                {
                    var vehicleData = await _redisDb.Multiplexer.GetDatabase().StringGetAsync(key);
                    if (!vehicleData.IsNullOrEmpty)
                    {
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
                _logger.LogError("Error retrieving vehicles: {Message}", ex.Message);
            }

            return vehicleList;
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(string id)
        {
            try
            {
                var key = $"vehicle:{id}";
                var vehicleData = await _redisDb.Multiplexer.GetDatabase().StringGetAsync(key);

                if (!vehicleData.IsNullOrEmpty)
                {
                    var vehicle = JsonSerializer.Deserialize<Vehicle>(vehicleData.ToString()!);
                    return vehicle;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving vehicle by ID {Id}: {Message}", id, ex.Message);
            }

            return null;
        }

        public async Task AddOrUpdateVehiclesAsync(List<Vehicle> vehicles)
        {
            foreach (var vehicle in vehicles)
            {
                var vehicleData = JsonSerializer.Serialize(vehicle);
                await _redisDb.Multiplexer.GetDatabase().StringSetAsync($"vehicle:{vehicle.VehicleID}", vehicleData);
                _logger.LogInformation("Vehicle {VehicleID} added/updated successfully.", vehicle.VehicleID);
            }
        }

        public async Task DeleteVehiclesAsync(string[] zoneIDs)
        {
            foreach (var id in zoneIDs)
            {
                await _redisDb.Multiplexer.GetDatabase().KeyDeleteAsync($"vehicle:{id}");
                _logger.LogInformation("Vehicle {id} deleted successfully.", id);
            }
        }

        #endregion

        public async Task<List<EvacuationStatus>> GetEvacuationStatusAsync()
        {
            var existingStatusData = await _redisDb.Multiplexer.GetDatabase().StringGetAsync("evacuation-statuses");
            var existingStatuses = string.IsNullOrEmpty(existingStatusData)
                ? []
                : JsonSerializer.Deserialize<List<EvacuationStatus>>(existingStatusData.ToString()!) ?? [];

            var finalStatuses = existingStatuses
                .GroupBy(s => s.ZoneID)
                .Select(g => g.OrderBy(s => s.RemainingPeople).FirstOrDefault())
                .Where(s => s != null)
                .Cast<EvacuationStatus>()
                .ToList();

            return finalStatuses;
        }

        public async Task ClearEvacuationPlansAsync()
        {
            await _redisDb.Multiplexer.GetDatabase().KeyDeleteAsync("evacuation-plans");
            await _redisDb.Multiplexer.GetDatabase().KeyDeleteAsync("evacuation-statuses");
        }

        private async Task SaveEvacuationPlanAsync(List<EvacuationPlan> plans)
        {
            await _redisDb.Multiplexer.GetDatabase().KeyDeleteAsync("evacuation-plans");
            var planData = JsonSerializer.Serialize(plans);
            await _redisDb.Multiplexer.GetDatabase().StringSetAsync("evacuation-plans", planData);
        }

        private async Task SaveLogEvacuationStatusesAsync(List<EvacuationStatus> statuses)
        {
            await _redisDb.Multiplexer.GetDatabase().KeyDeleteAsync("evacuation-statuses");
            var statusData = JsonSerializer.Serialize(statuses);
            await _redisDb.Multiplexer.GetDatabase().StringSetAsync("evacuation-statuses", statusData);
        }

        public async Task AppendEvacuationStatusesAsync(List<EvacuationStatus> newStatuses)
        {
            var existingStatusData = await _redisDb.Multiplexer.GetDatabase().StringGetAsync("evacuation-statuses");
            var existingStatuses = string.IsNullOrEmpty(existingStatusData)
                ? []
                : JsonSerializer.Deserialize<List<EvacuationStatus>>(existingStatusData.ToString()!) ?? [];

            foreach (var newStatus in newStatuses)
            {
                if (!existingStatuses.Any(s => s.ZoneID == newStatus.ZoneID && s.TotalEvacuated == newStatus.TotalEvacuated))
                {
                    existingStatuses.Add(newStatus);
                }
            }

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
                var availableVehicles = vehicles
                    .Where(v => v.Capacity > 0)
                    .OrderByDescending(v => v.Capacity)
                    .ThenBy(v => vehicleDistances[v.VehicleID][zone.ZoneID])
                    .Except(usedVehicles)
                    .ToList();

                var bestVehicle = availableVehicles.FirstOrDefault();
                if (bestVehicle == null)
                {
                    _logger.LogWarning("No available vehicle for zone {ZoneID}. Skipping this zone.", zone.ZoneID);
                    break;
                }

                var eta = vehicleETAs[bestVehicle.VehicleID][zone.ZoneID];
                
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
                    _logger.LogInformation("Processing zone {ZoneID}: {NumberOfPeople} people remaining...", zone.ZoneID, zone.NumberOfPeople);
                    int evacueesToTransport = Math.Min(remainingPeople, bestVehicle.Capacity);
                    remainingPeople -= evacueesToTransport;
                    remainingPeople = Math.Max(0, remainingPeople);

                    var evacuationStatus = new EvacuationStatus
                    {
                        ZoneID = zone.ZoneID,
                        TotalEvacuated = (zone.NumberOfPeople - remainingPeople),
                        RemainingPeople = remainingPeople,
                        LastVehicleUsed = bestVehicle.VehicleID
                    };
                    evacuationStatuses.Add(evacuationStatus);

                    _logger.LogInformation("Evacuated {evacueesToTransport} people from zone {ZoneID} using vehicle {VehicleID}. Remaining: {remainingPeople}.", evacueesToTransport, zone.ZoneID, bestVehicle.VehicleID, remainingPeople);

                    if (remainingPeople <= 0)
                    {
                        _logger.LogInformation("Zone {ZoneID} has been fully evacuated.", zone.ZoneID);
                        break;
                    }
                }

                usedVehicles.Add(bestVehicle);
            }

            await SaveEvacuationPlanAsync(evacuationPlans);
            await SaveLogEvacuationStatusesAsync(evacuationStatuses);

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

            zones = [.. zones.OrderByDescending(z => z.UrgencyLevel)];
            var evacuationPlans = new List<EvacuationPlan>();
            var evacuationStatuses = new List<EvacuationStatus>();

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
                var availableVehicles = vehicles
                    .Where(v => v.Capacity > 0)
                    .OrderByDescending(v => v.Capacity)
                    .ThenBy(v => vehicleDistances[v.VehicleID][zone.ZoneID])
                    .Except(usedVehicles)
                    .ToList();

                var bestVehicle = availableVehicles.FirstOrDefault();

                if (bestVehicle == null)
                {
                    _logger.LogWarning("No available vehicle for zone {ZoneID}. Skipping this zone.", zone.ZoneID);
                    continue;
                }

                var evacuationPlan = new EvacuationPlan
                {
                    ZoneID = zone.ZoneID,
                    VehicleID = bestVehicle.VehicleID,
                    NumberOfPeople = zone.NumberOfPeople,
                    ETA = $"{vehicleETAs[bestVehicle.VehicleID][zone.ZoneID].Hours}h {vehicleETAs[bestVehicle.VehicleID][zone.ZoneID].Minutes}m per round"
                };
                evacuationPlans.Add(evacuationPlan);

                var evacuationStatus = new EvacuationStatus
                {
                    ZoneID = zone.ZoneID,
                    TotalEvacuated = 0,
                    RemainingPeople = zone.NumberOfPeople,
                    LastVehicleUsed = bestVehicle.VehicleID
                };
                evacuationStatuses.Add(evacuationStatus);

                _logger.LogInformation("Evacuated {evacueesToTransport} people from zone {zoneID} using vehicle {VehicleID}. Remaining: {evacuees}.", 0, zone.ZoneID, bestVehicle.VehicleID, zone.NumberOfPeople);

                usedVehicles.Add(bestVehicle);
            }

            await SaveEvacuationPlanAsync(evacuationPlans);
            await SaveLogEvacuationStatusesAsync(evacuationStatuses);

            return evacuationPlans;
        }

        private async Task<List<EvacuationPlan>> GetEvacuationPlansAsync()
        {
            var existingPlansData = await _redisDb.Multiplexer.GetDatabase().StringGetAsync("evacuation-plans");
            var existingPlans = string.IsNullOrEmpty(existingPlansData)
                ? []
                : JsonSerializer.Deserialize<List<EvacuationPlan>>(existingPlansData.ToString()!) ?? [];

            return existingPlans;
        }

        public async Task<EvacuationStatus> UpdateEvacuationAsync(string zoneID, int evacuees)
        {
            var evacuationPlans = await GetEvacuationPlansAsync();
            var plan = evacuationPlans.FirstOrDefault(p => p.ZoneID == zoneID) ?? throw new Exception($"No evacuation plan found for zone {zoneID}.");

            var vehicle = await GetVehicleByIdAsync(plan.VehicleID) ?? throw new Exception($"No vehicle found with ID {plan.VehicleID}.");

            var allStatuses = await GetEvacuationStatusAsync();
            var filteredStatuses = allStatuses
                .Where(status => status.ZoneID.Equals(zoneID, StringComparison.OrdinalIgnoreCase))
                .First();

            int evacueesToTransport = Math.Min(evacuees, vehicle.Capacity);
            filteredStatuses.RemainingPeople -= evacueesToTransport;
            if (filteredStatuses.RemainingPeople < 0)
            {
                evacueesToTransport += filteredStatuses.RemainingPeople;
            }            
            filteredStatuses.RemainingPeople = Math.Max(0, filteredStatuses.RemainingPeople);

            // Create the evacuation status
            var evacuationStatus = new EvacuationStatus
            {
                ZoneID = zoneID,
                TotalEvacuated = (evacueesToTransport + filteredStatuses.TotalEvacuated),
                RemainingPeople = filteredStatuses.RemainingPeople,
                LastVehicleUsed = plan.VehicleID
            };

            _logger.LogInformation("Evacuated {evacueesToTransport} people from zone {zoneID} using vehicle {VehicleID}. Remaining: {evacuees}.", evacueesToTransport, zoneID, plan.VehicleID, evacuees);

            await AppendEvacuationStatusesAsync([evacuationStatus]);
            
            return evacuationStatus;
        }
    }
}