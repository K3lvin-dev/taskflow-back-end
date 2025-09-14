using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskFlowAPI.src.entity.user.models;

namespace TaskFlowAPI.src.entity.user.services;

public interface IJwtService
{
    string GenerateToken(User user);
    ClaimsPrincipal? ValidateToken(string token);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly TimeSpan _expiration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
        _secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");
        _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
        _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

        var expirationMinutes = Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES");
        _expiration = TimeSpan.FromMinutes(int.TryParse(expirationMinutes, out var minutes) ? minutes : 60);
    }

    public string GenerateToken(User user)
    {
        var key = Encoding.UTF8.GetBytes(_secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("user_id", user.Id.ToString())
            }),
            Expires = DateTime.UtcNow.Add(_expiration),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}