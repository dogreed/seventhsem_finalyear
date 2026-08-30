using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Interfaces;
using FinalProject_SeventhSem.Application.Models.Auth;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IRepository<FinalProject_SeventhSem.Domain.Entities.RefreshToken> _refreshTokenRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly ITokenService _tokenService;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(
        IRepository<FinalProject_SeventhSem.Domain.Entities.RefreshToken> refreshTokenRepo,
        IRepository<User> userRepo,
        IUnitOfWork uow,
        ITokenService tokenService,
        IJwtService jwtService)
    {
        _refreshTokenRepo = refreshTokenRepo;
        _userRepo = userRepo;
        _uow = uow;
        _tokenService = tokenService;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Derive lookup key — fetches a single row instead of scanning all tokens with BCrypt
        var lookupKey = _tokenService.GenerateLookupKey(request.RawRefreshToken);

        var allTokens = await _refreshTokenRepo.GetAllAsync(cancellationToken);

        // Narrow to the single candidate row using the plain-text lookup key
        var candidate = allTokens.FirstOrDefault(t => t.TokenLookup == lookupKey);

        if (candidate is not null && candidate.IsRevoked)
        {
            // Security breach: token was already rotated — revoke entire user chain
            var chain = allTokens.Where(t => t.UserId == candidate.UserId).ToList();
            foreach (var t in chain) t.IsRevoked = true;
            await _uow.SaveChangesAsync(cancellationToken);
            throw new InvalidRefreshTokenException("Token reuse detected. All sessions revoked.");
        }

        var stored = candidate is not null
            && !candidate.IsRevoked
            && candidate.ExpiresAt > DateTime.UtcNow
            && _tokenService.VerifyToken(request.RawRefreshToken, candidate.TokenHash)
                ? candidate : null;

        if (stored is null)
            throw new InvalidRefreshTokenException();

        var user = await _userRepo.GetByIdAsync(stored.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), stored.UserId);

        // Rotate: revoke old, issue new
        stored.IsRevoked = true;

        var rawNew = _tokenService.GenerateRawRefreshToken();
        var newToken = new FinalProject_SeventhSem.Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokenService.HashToken(rawNew),
            TokenLookup = _tokenService.GenerateLookupKey(rawNew),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        await _refreshTokenRepo.AddAsync(newToken, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        stored.ReplacedByTokenId = newToken.Id;
        await _uow.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            AccessToken: _jwtService.GenerateAccessToken(user),
            RefreshToken: rawNew,
            AccessTokenExpiresAt: DateTime.UtcNow.AddMinutes(15),
            User: new UserSummary(user.Id,user.Student?.FullName ?? user.Organization?.Name?? string.Empty, user.Email, user.Role.ToString()));
    }
}