using EvacuationAPI.Models;
using EvacuationAPI.Services;
using EvacuationAPI.Services.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EvacuationAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class EvacuationController(EvacuationService evacuationService, ILogger<EvacuationService> logger) : ControllerBase
    {
        private readonly EvacuationService _evacuationService = evacuationService;
        private readonly ILogger<EvacuationService> _logger = logger;

        //[HttpGet("redis-health")]
        //public async Task<IActionResult> CheckRedisConnection()
        //{
        //    try
        //    {
        //        bool value = _evacuationService.CheckRedisConnection();
        //        return Ok(value? "Redis is connected" : "Redis connection failed");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Error updating evacuation zones: {Message}", ex.Message);
        //        return StatusCode(500, $"Error connecting to Redis: {ex.Message}");
        //    }
        //}

        //[HttpGet("redis-keys")]
        //public async Task<IActionResult> GetRedisInfo()
        //{
        //    try
        //    {
        //        var keys = await _evacuationService.GetKeysAsync();
        //        return Ok(keys);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Error retrieving keys from Redis: {Message}", ex.Message);
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        #region Evacuation Zone

        /// <summary>
        /// POST /api/evacuation-zones: Adds information about evacuation zones.
        /// </summary>
        /// <param name="zones"></param>
        /// <returns></returns>
        [HttpPost("evacuation-zones")]
        public async Task<IActionResult> AddOrUpdateEvacuationZones([FromBody] List<EvacuationZone> zones)
        {
            try
            {
                await _evacuationService.AddOrUpdateEvacuationZonesAsync(zones);
                return Ok("Evacuation zones updated");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating evacuation zones: {Message}", ex.Message);
                return StatusCode(500, "Error updating evacuation zones");
            }
        }

        /// <summary>
        /// GET api/evacuation/zones: List all evacuation zones.
        /// </summary>
        /// <returns></returns>
        [HttpGet("evacuation-zones")]
        public async Task<ActionResult<IEnumerable<EvacuationZone>>> GetAllEvacuationZonesAsync()
        {
            try
            {
                var zones = await _evacuationService.GetAllEvacuationZonesAsync();
                return Ok(zones);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving evacuation zones: {Message}", ex.Message);
                return StatusCode(500, "Error retrieving evacuation zones");
            }
        }

        /// <summary>
        /// GET api/evacuation/zones/{id}: Get evacuation zone by ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("evacuation-zones/{id}")]
        public async Task<ActionResult<EvacuationZone>> GetEvacuationZoneByIdAsync(string id)
        {
            try
            {
                var zone = await _evacuationService.GetEvacuationZoneByIdAsync(id);
                if (zone == null)
                {
                    _logger.LogError("Evacuation zone ID: {ID} not found", id);
                    return NotFound("Evacuation zone ID: {ID} not found");
                }
                return Ok(zone);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving this evacuation zone: {Message}", ex.Message);
                return StatusCode(500, "Error retrieving this evacuation zone");
            }
        }

        /// <summary>
        /// DELETE /api/evacuation-zones: Delete information from evacuation zones by array of ID.
        /// </summary>
        /// <param name="zoneIDs"></param>
        /// <returns></returns>
        [HttpDelete("evacuation-zones")]
        public async Task<IActionResult> DeleteEvacuationZones([FromBody] string[] zoneIDs)
        {
            try
            {
                await _evacuationService.DeleteEvacuationZonesAsync(zoneIDs);
                return Ok("Evacuation zones deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting evacuation zones: {Message}", ex.Message);
                return StatusCode(500, "Error deleting evacuation zones");
            }
        }

        #endregion

        #region Vehicle

        /// <summary>
        /// POST /api/vehicles: Adds information about available vehicles.
        /// </summary>
        /// <param name="vehicles"></param>
        /// <returns></returns>
        [HttpPost("vehicles")]
        public async Task<IActionResult> AddOrUpdateVehicles([FromBody] List<Vehicle> vehicles)
        {
            try
            {
                await _evacuationService.AddOrUpdateVehiclesAsync(vehicles);
                return Ok("Vehicles updated");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating vehicles: {Message}", ex.Message);
                return StatusCode(500, "Error updating vehicles");
            }
        }

        /// <summary>
        /// GET api/evacuation/vehicles: List all vehicles.
        /// </summary>
        /// <returns></returns>
        [HttpGet("vehicles")]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetAllVehiclesAsync()
        {
            try
            {
                var vehicles = await _evacuationService.GetAllVehiclesAsync();
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving vehicles: {Message}", ex.Message);
                return StatusCode(500, "Error retrieving vehicles");
            }
        }

        /// <summary>
        /// GET api/evacuation/vehicles/{id}: Get vehicle by ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("vehicles/{id}")]
        public async Task<ActionResult<Vehicle>> GetVehicleByIdAsync(string id)
        {
            try
            {
                var vehicle = await _evacuationService.GetVehicleByIdAsync(id);
                if (vehicle == null)
                {
                    _logger.LogError("Vehicle ID: {ID} not found", id);
                    return NotFound("Vehicle ID: {ID} not found");
                }
                return Ok(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving this vehicle: {Message}", ex.Message);
                return StatusCode(500, "Error retrieving this vehicle");
            }
        }

        /// <summary>
        /// DELETE /api/vehicles: Delete information from vehicles by array of ID.
        /// </summary>
        /// <param name="vehicleIDs"></param>
        /// <returns></returns>
        [HttpDelete("vehicles")]
        public async Task<IActionResult> DeleteVehicles([FromBody] string[] vehicleIDs)
        {
            try
            {
                await _evacuationService.DeleteEvacuationZonesAsync(vehicleIDs);
                return Ok("Vehicles deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting vehicles: {Message}", ex.Message);
                return StatusCode(500, "Error deleting vehicles");
            }
        }

        #endregion

        #region Evacuate

        /// <summary>
        /// POST /api/evacuations/plan: Generates plans that assigns vehicles to evacuation zones.
        /// </summary>
        /// <returns>A list of new or updated evacuation plans</returns>
        [HttpPost("evacuations/plan")]
        public async Task<IActionResult> GenerateEvacuationPlan()
        {
            try
            {
                var plans = await _evacuationService.PlanEvacuationAsync();
                return Ok(plans);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error generating evacuation plan: {Message}", ex.Message);
                return StatusCode(500, "Error generating evacuation plan");
            }
        }

        /// <summary>
        /// GET /api/evacuations/status: Returns the current status of all evacuation zones.
        /// </summary>
        /// <returns></returns>
        [HttpGet("evacuations/status")]
        public async Task<IActionResult> GetEvacuationStatus()
        {
            try
            {
                var statusList = await _evacuationService.GetEvacuationStatusAsync();
                return Ok(statusList);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving evacuation status: {Message}", ex.Message);
                return StatusCode(500, "Error retrieving evacuation status");
            }
        }

        /// <summary>
        /// PUT /api/evacuations/update: Updates the evacuation status.
        /// </summary>
        /// <param name="zoneID"></param>
        /// <param name="evacuees"></param>
        /// <returns></returns>
        [HttpPut("evacuations/update")]
        public async Task<IActionResult> UpdateEvacuationStatus(string zoneID, int evacuees)
        {
            try
            {
                var status = await _evacuationService.UpdateEvacuationAsync(zoneID, evacuees);
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating evacuation status: {Message}", ex.Message);
                return StatusCode(500, "Error updating evacuation status");
            }
        }

        /// <summary>
        /// DELETE /api/evacuations/clear: Clears all current evacuation plans.
        /// </summary>
        /// <returns></returns>
        [HttpDelete("evacuations/clear")]
        public async Task<IActionResult> ClearEvacuationPlans()
        {
            try
            {
                await _evacuationService.ClearEvacuationPlansAsync();
                return Ok("Evacuation plans cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error clearing evacuation plans: {Message}", ex.Message);
                return StatusCode(500, "Error clearing evacuation plans");
            }
        }

        #endregion

        #region Others
        /// <summary>
        /// POST /api/evacuations/autoplan: Generates full auto plan that assigns vehicles to evacuation zones.
        /// </summary>
        /// <returns>A list of new or updated evacuation plans</returns>
        [HttpPost("evacuations/autoplan")]
        //[ApiExplorerSettings(GroupName = "Others")]
        public async Task<IActionResult> AutoGenerateEvacuationPlan()
        {
            try
            {
                var plans = await _evacuationService.AutoGenerateEvacuationPlanAsync();
                return Ok(plans);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error auto generating evacuation plan: {Message}", ex.Message);
                return StatusCode(500, "Error auto generating evacuation plan");
            }
        }

        #endregion
    }
}
