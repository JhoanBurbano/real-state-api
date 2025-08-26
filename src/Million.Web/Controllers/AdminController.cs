using Microsoft.AspNetCore.Mvc;
using Million.Application.DTOs.Auth;
using Million.Application.Interfaces;
using Million.Domain.Entities;
using Million.Domain.Exceptions;
using Million.Web.Middlewares;

namespace Million.Web.Controllers;

[ApiController]
[Route("admin")]
[Tags("Admin")]
[RequiresAuth(requireAdmin: true)]
public class AdminController : ControllerBase
{
    private readonly IAuthService _authService;

    public AdminController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Get all owners with pagination
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of owners</returns>
    [HttpGet("owners")]
    [ProducesResponseType(typeof(List<Owner>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOwners(
        [FromQuery] string? query = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var owners = await _authService.GetOwnersAsync(query, page, pageSize, HttpContext.RequestAborted);
        return Ok(owners);
    }

    /// <summary>
    /// Get owner by ID
    /// </summary>
    /// <param name="id">Owner ID</param>
    /// <returns>Owner details</returns>
    [HttpGet("owners/{id}")]
    [ProducesResponseType(typeof(Owner), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOwner(string id)
    {
        var owner = await _authService.GetOwnerByIdAsync(id, HttpContext.RequestAborted);
        if (owner == null)
        {
            return NotFound(new { message = $"Owner with ID '{id}' not found" });
        }
        return Ok(owner);
    }

    /// <summary>
    /// Get owner sessions
    /// </summary>
    /// <param name="id">Owner ID</param>
    /// <returns>List of owner sessions</returns>
    [HttpGet("owners/{id}/sessions")]
    [ProducesResponseType(typeof(List<OwnerSession>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOwnerSessions(string id)
    {
        var owner = await _authService.GetOwnerByIdAsync(id, HttpContext.RequestAborted);
        if (owner == null)
        {
            return NotFound(new { message = $"Owner with ID '{id}' not found" });
        }

        var sessions = await _authService.GetOwnerSessionsAsync(id, HttpContext.RequestAborted);
        return Ok(sessions);
    }

    /// <summary>
    /// Create new owner
    /// </summary>
    /// <param name="request">Owner creation data</param>
    /// <returns>Created owner</returns>
    [HttpPost("owners")]
    [ProducesResponseType(typeof(Owner), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateOwner([FromBody] CreateOwnerRequest request)
    {
        try
        {
            var owner = await _authService.CreateOwnerAsync(request, HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetOwner), new { id = owner.Id }, owner);
        }
        catch (DuplicateEmailException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update owner
    /// </summary>
    /// <param name="id">Owner ID</param>
    /// <param name="request">Update data</param>
    /// <returns>Updated owner</returns>
    [HttpPatch("owners/{id}")]
    [ProducesResponseType(typeof(Owner), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateOwner(string id, [FromBody] UpdateOwnerRequest request)
    {
        try
        {
            var owner = await _authService.UpdateOwnerAsync(id, request, HttpContext.RequestAborted);
            return Ok(owner);
        }
        catch (OwnerNotFoundException)
        {
            return NotFound(new { message = $"Owner with ID '{id}' not found" });
        }
    }

    /// <summary>
    /// Revoke owner session
    /// </summary>
    /// <param name="id">Owner ID</param>
    /// <param name="sid">Session ID</param>
    /// <returns>No content</returns>
    [HttpPost("owners/{id}/revoke-session/{sid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RevokeSession(string id, string sid)
    {
        await _authService.RevokeSessionAsync(sid, HttpContext.RequestAborted);
        return NoContent();
    }
}
