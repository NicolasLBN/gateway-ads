using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BlazorApp.Models;
using Microsoft.IdentityModel.Tokens;

namespace BlazorApp.Services;

public class JwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public string CreateToken(AuthUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetSecret()));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresHours = _config.GetValue("Jwt:ExpiresHours", 12);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "gateway-ads",
            audience: _config["Jwt:Audience"] ?? "gateway-ads-clients",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiresHours),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GetSecret() =>
        _config["Jwt:Secret"] ?? "GatewayAdsDevSecretKey_ChangeMe_32chars!";
}
