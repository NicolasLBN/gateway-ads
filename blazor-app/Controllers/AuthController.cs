using System.ComponentModel.DataAnnotations;
using BlazorApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorApp.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly JwtTokenService _jwt;

    public AuthController(AuthService auth, JwtTokenService jwt)
    {
        _auth = auth;
        _jwt = jwt;
    }

    public record LoginRequest(
        [property: Required] string Username,
        [property: Required] string Password);

    public record LoginResponse(string Token, string Username, DateTime ExpiresAtUtc);

    [HttpPost("login")]
    [AllowAnonymous]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var result = _auth.Login(request.Username, request.Password);
        if (!result.Success || result.User == null)
            return Unauthorized(new { error = result.Error ?? "Invalid credentials" });

        var token = _jwt.CreateToken(result.User);
        return Ok(new LoginResponse(token, result.User.Username, DateTime.UtcNow.AddHours(12)));
    }
}
