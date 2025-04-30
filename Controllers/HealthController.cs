using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CivicTransportCard.Controllers
{
    [ApiController]
    [Route("api")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Returns a simple health check payload.
        /// </summary>
        /// <returns>
        /// 200 OK with a JSON object containing:
        /// - status: always "Healthy"  
        /// - timestamp: UTC now
        /// </returns>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get API health status",
            Description = "Checks that the API is up and returns a timestamped payload."
        )]
        [ProducesResponseType(typeof(HealthResponseDto), StatusCodes.Status200OK)]
        public IActionResult GetHealth()
        {
            return Ok(GetHealthPayload());
        }

        private object GetHealthPayload() => new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow
        };
    }
}
