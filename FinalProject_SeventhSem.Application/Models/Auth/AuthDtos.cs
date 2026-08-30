using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Models.Auth;

public record RegisterStudentRequest(
    string FullName,
    string Email,
    string Password
);

public record RegisterOrganizationRequest(
    string OrganizationName,
    string Email,
    string Password,
    string? WebsiteUrl
);

public record LoginRequest(
    string Email,
    string Password
);

public record RefreshTokenRequest(
    string RefreshToken
);


public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    UserSummary User
);

public record UserSummary(
    int UserId,
    string fullName,
    string Email,
    string Role
);