using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Interfaces;

public interface ITokenService
{
    string GenerateRawRefreshToken();

    string GenerateLookupKey(string rawToken);

    string HashToken(string rawToken);

    bool VerifyToken(string rawToken, string hash);
}
