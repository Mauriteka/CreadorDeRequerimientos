using System.Security.Claims;
using CreadorDeRequerimientos.API.Auth;
using CreadorDeRequerimientos.API.Controllers.Base;
using CreadorDeRequerimientos.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace CreadorDeRequerimientos.API.Controllers;

[Route("api/auth")]
public sealed class AuthController(WorkspaceAuthService authService) : ApiController
{
    [HttpGet("status")]
    public ActionResult<AuthStatusResponse> GetStatus()
    {
        var isAuthenticated = User.Identity?.IsAuthenticated == true;
        return Ok(new AuthStatusResponse(
            authService.IsEnabled,
            isAuthenticated,
            isAuthenticated ? User.Identity?.Name : null));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthStatusResponse>> Login(LoginRequest request)
    {
        if (!authService.IsEnabled)
        {
            return BadRequest(new { message = "La autenticacion no esta configurada." });
        }

        if (!authService.ValidateCredentials(request.Username, request.Password))
        {
            return Unauthorized();
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, authService.Username)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Ok(new AuthStatusResponse(true, true, authService.Username));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<AuthStatusResponse>> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new AuthStatusResponse(authService.IsEnabled, false, null));
    }
}
