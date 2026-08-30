using FinalProject_SeventhSem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    int? GetUserIdFromToken(string token);
}

