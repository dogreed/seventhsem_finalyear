using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Interfaces;
using FinalProject_SeventhSem.Application.Models.Auth;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Enums;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<FinalProject_SeventhSem.Domain.Entities.RefreshToken> _refreshTokenRepo;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        IRepository<User> userRepo,
        IRepository<FinalProject_SeventhSem.Domain.Entities.RefreshToken> refreshTokenRepo,
        IUnitOfWork uow,
        IPasswordService passwordService,
        IJwtService jwtService,
        ITokenService tokenService)
    {
        _userRepo = userRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _uow = uow;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        //var user = (await _userRepo.GetAllAsync(cancellationToken))
        //    .FirstOrDefault(u => u.Email == request.Email.ToLower());

        var users = await _userRepo.GetAllAsync(
                include: q => q.Include(u => u.Organization)
                .Include(u=> u.Student),
                cancellationToken: cancellationToken);

        var user = users.FirstOrDefault(u => u.Email == request.Email.ToLower());

        if (user is null || !_passwordService.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        if (!user.IsActive)
            throw new OrganizationNotVerifiedException();

        // For Organization role, double-check IsVerified
        if (user.Role == UserRole.Organization)
        {
            var org = user.Organization;
            if (org is null || org.Status != OrganizationStatus.Verified)
                throw new OrganizationNotVerifiedException();
        }

        var rawRefresh = _tokenService.GenerateRawRefreshToken();
        var refreshToken = new FinalProject_SeventhSem.Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokenService.HashToken(rawRefresh),
            TokenLookup = _tokenService.GenerateLookupKey(rawRefresh),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        await _refreshTokenRepo.AddAsync(refreshToken, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            AccessToken: _jwtService.GenerateAccessToken(user),
            RefreshToken: rawRefresh,
            AccessTokenExpiresAt: DateTime.UtcNow.AddMinutes(15),
            User: new UserSummary(user.Id, user.Student?.FullName ?? user.Organization?.Name?? string.Empty, user.Email, user.Role.ToString()));
    }
}

