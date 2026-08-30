using FinalProject_SeventhSem.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? User
        => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated
        => User?.Identity?.IsAuthenticated ?? false;

    public int UserId
    {
        get
        {
            var sub = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User?.FindFirstValue("sub");
            return int.TryParse(sub, out var id) ? id : 0;
        }
    }

    public string Role
        => User?.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
}
