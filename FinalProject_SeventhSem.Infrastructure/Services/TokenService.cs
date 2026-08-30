using FinalProject_SeventhSem.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure.Services;

public class TokenService : ITokenService
{
    public string GenerateRawRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public string GenerateLookupKey(string rawToken)
        => rawToken.Length >= 16 ? rawToken[..16] : rawToken;

    public string HashToken(string rawToken)
        => BCrypt.Net.BCrypt.HashPassword(rawToken, workFactor: 11);

    public bool VerifyToken(string rawToken, string hash)
        => BCrypt.Net.BCrypt.Verify(rawToken, hash);
}