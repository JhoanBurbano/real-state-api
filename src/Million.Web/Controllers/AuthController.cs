using Microsoft.AspNetCore.Mvc;
using Million.Application.DTOs.Auth;
using Million.Application.Interfaces;
using Million.Domain.Exceptions;
using Million.Web.Middlewares;

namespace Million.Web.Controllers;

[ApiController]
[Route("auth")]
[Tags("Authentication")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Owner login
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT tokens</returns>
    [HttpPost("owner/login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var clientIp = GetClientIp();
            var userAgent = Request.Headers["User-Agent"].FirstOrDefault();

            var response = await _authService.LoginAsync(request, clientIp, userAgent, HttpContext.RequestAborted);
            return Ok(response);
        }
        catch (InvalidCredentialsException)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }
        catch (AccountLockedException)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new { message = "Account temporarily locked" });
        }
        catch (AccountInactiveException)
        {
            return Unauthorized(new { message = "Account is inactive" });
        }
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>New JWT tokens</returns>
    [HttpPost("owner/refresh")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        try
        {
            var clientIp = GetClientIp();
            var userAgent = Request.Headers["User-Agent"].FirstOrDefault();

            var response = await _authService.RefreshAsync(request, clientIp, userAgent, HttpContext.RequestAborted);
            return Ok(response);
        }
        catch (RefreshTokenRevokedException)
        {
            return Unauthorized(new { message = "Refresh token has been revoked" });
        }
        catch (TokenExpiredException)
        {
            return Unauthorized(new { message = "Refresh token has expired" });
        }
    }

    /// <summary>
    /// Owner logout
    /// </summary>
    /// <param name="request">Refresh token to revoke</param>
    /// <returns>No content</returns>
    [HttpPost("owner/logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
        await _authService.LogoutAsync(request.RefreshToken, HttpContext.RequestAborted);
        return NoContent();
    }

    private string GetClientIp()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
