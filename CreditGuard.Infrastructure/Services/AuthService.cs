using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CreditGuard.Core.Entities;
using CreditGuard.Core.Interfaces;
using CreditGuard.Core.Utilities;
using CreditGuard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CreditGuard.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<string> RegisterAsync(string username, string phone, string firstName, string lastName, string address, string password)
    {
        var exists = await _context.Creditors.AnyAsync(c => c.Username == username);
        if (exists) throw new Exception("Username is already taken.");

        var creditor = new Creditor
        {
            Username = username,
            Phone = phone,
            FirstName = firstName,
            LastName = lastName,
            Address = address,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateHelper.GetCurrentUnixTimeSeconds()
        };

        _context.Creditors.Add(creditor);
        await _context.SaveChangesAsync();

        return GenerateJwtToken(creditor);
    }

    public async Task<string> LoginAsync(string username, string password)
    {
        var creditor = await _context.Creditors.FirstOrDefaultAsync(c => c.Username == username);
        if (creditor == null || !BCrypt.Net.BCrypt.Verify(password, creditor.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        return GenerateJwtToken(creditor);
    }

    private string GenerateJwtToken(Creditor creditor)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, creditor.Id),
            new Claim(ClaimTypes.NameIdentifier, creditor.Id),
            new Claim(ClaimTypes.MobilePhone, creditor.Phone ?? ""),
            new Claim(ClaimTypes.Name, creditor.Username),
            new Claim("fullName", $"{creditor.FirstName} {creditor.LastName}")
        };

        var secret = _config["Jwt:SecretKey"] ?? "super-secret-key-that-should-be-very-long";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "CreditGuard",
            audience: _config["Jwt:Audience"] ?? "CreditGuardApp",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
