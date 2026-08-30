using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Interfaces;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Enums;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Auth.Commands.RegisterOrganization;

public class RegisterOrganizationCommandHandler
    : IRequestHandler<RegisterOrganizationCommand, string>
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Organization> _orgRepo;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordService _passwordService;

    public RegisterOrganizationCommandHandler(
        IRepository<User> userRepo,
        IRepository<Organization> orgRepo,
        IUnitOfWork uow,
        IPasswordService passwordService)
    {
        _userRepo = userRepo;
        _orgRepo = orgRepo;
        _uow = uow;
        _passwordService = passwordService;
    }

    public async Task<string> Handle(
        RegisterOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        var existing = (await _userRepo.GetAllAsync(cancellationToken))
            .FirstOrDefault(u => u.Email == request.Email.ToLower());
        if (existing is not null)
            throw new ConflictException($"Email '{request.Email}' is already registered.");

        var user = new User
        {
            Email = request.Email.ToLower(),
            PasswordHash = _passwordService.Hash(request.Password),
            Role = UserRole.Organization,
            IsActive = false // cannot login until Admin verifies
        };
        await _userRepo.AddAsync(user, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var org = new Organization
        {
            UserId = user.Id,
            Name = request.OrganizationName,
            WebsiteUrl = request.WebsiteUrl,
            Status = OrganizationStatus.Pending
        };
        await _orgRepo.AddAsync(org, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return "Organization registered successfully. Await admin verification before logging in.";
    }
}

