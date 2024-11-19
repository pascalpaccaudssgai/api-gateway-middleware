using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApiMappingController : ControllerBase
{
    private readonly IApiConfigurationService _configService;
    private readonly ILogger<ApiMappingController> _logger;

    public ApiMappingController(
        IApiConfigurationService configService,
        ILogger<ApiMappingController> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApiMapping>>> GetAllMappings()
    {
        try
        {
            var mappings = await _configService.GetAllMappings();
            return Ok(mappings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving API mappings");
            return StatusCode(500, "An error occurred while retrieving API mappings");
        }
    }

    [HttpGet("{endpoint}/{method}")]
    public async Task<ActionResult<ApiMapping>> GetMapping(string endpoint, string method)
    {
        try
        {
            var mapping = await _configService.GetMappingForEndpoint(endpoint, method);
            if (mapping == null)
            {
                return NotFound();
            }
            return Ok(mapping);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving API mapping for {Endpoint} {Method}", endpoint, method);
            return StatusCode(500, "An error occurred while retrieving the API mapping");
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiMapping>> AddMapping([FromBody] ApiMapping mapping)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _configService.AddMapping(mapping);
            if (!success)
            {
                return Conflict("A mapping for this endpoint and method already exists");
            }

            return CreatedAtAction(nameof(GetMapping), 
                new { endpoint = mapping.SourceEndpoint, method = mapping.HttpMethod }, 
                mapping);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding API mapping");
            return StatusCode(500, "An error occurred while adding the API mapping");
        }
    }

    [HttpPut]
    public async Task<ActionResult> UpdateMapping([FromBody] ApiMapping mapping)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _configService.UpdateMapping(mapping);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating API mapping");
            return StatusCode(500, "An error occurred while updating the API mapping");
        }
    }

    [HttpDelete("{endpoint}/{method}")]
    public async Task<ActionResult> DeleteMapping(string endpoint, string method)
    {
        try
        {
            var success = await _configService.DeleteMapping(endpoint, method);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting API mapping for {Endpoint} {Method}", endpoint, method);
            return StatusCode(500, "An error occurred while deleting the API mapping");
        }
    }
}
