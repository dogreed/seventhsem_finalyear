using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Interfaces;
using FinalProject_SeventhSem.Application.Models.Auth;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Enums;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Auth.Commands.RegisterStudent;

public class RegisterStudentCommandHandler
    : IRequestHandler<RegisterStudentCommand, AuthResponse>
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Student> _studentRepo;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly ITokenService _tokenService;
    private readonly IRepository<FinalProject_SeventhSem.Domain.Entities.RefreshToken> _refreshTokenRepo;

    public RegisterStudentCommandHandler(
        IRepository<User> userRepo,
        IRepository<Student> studentRepo,
        IUnitOfWork uow,
        IPasswordService passwordService,
        IJwtService jwtService,
        ITokenService tokenService,
        IRepository<FinalProject_SeventhSem.Domain.Entities.RefreshToken> refreshTokenRepo)
    {
        _userRepo = userRepo;
        _studentRepo = studentRepo;
        _uow = uow;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _tokenService = tokenService;
        _refreshTokenRepo = refreshTokenRepo;
    }

    public async Task<AuthResponse> Handle(
        RegisterStudentCommand request,
        CancellationToken cancellationToken)
    {
        // Conflict guard — duplicate email
        try
        {
			var existing = (await _userRepo.GetAllAsync(cancellationToken))
	        .FirstOrDefault(u => u.Email == request.Email.ToLower());
			if (existing is not null)
				throw new ConflictException($"Email '{request.Email}' is already registered.");
		}
		catch (Exception ex)
        {
            throw new Exception($"An error occurred while checking for existing email: {ex.Message}", ex);
		}

        

        var user = new User
        {
            Email = request.Email.ToLower(),
            PasswordHash = _passwordService.Hash(request.Password),
            Role = UserRole.Student
        };
        await _userRepo.AddAsync(user, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken); // get user.Id

        var student = new Student { UserId = user.Id, FullName = request.FullName };
        await _studentRepo.AddAsync(student, cancellationToken);

        // Issue tokens
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

