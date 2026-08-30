using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Enums;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Admin.Commands.VerifyOrganization;

public class VerifyOrganizationCommandHandler : IRequestHandler<VerifyOrganizationCommand, VerifyOrganizationResponse>
{
    private readonly IRepository<Organization> _orgRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IUnitOfWork _uow;

    public VerifyOrganizationCommandHandler(
        IRepository<Organization> orgRepo,
        IRepository<User> userRepo,
        IUnitOfWork uow)
    {
        _orgRepo = orgRepo;
        _userRepo = userRepo;
        _uow = uow;
    }

    //public async Task<VerifyOrganizationResponse> Handle(VerifyOrganizationCommand request, CancellationToken cancellationToken)
    //{
    //    var org = await _orgRepo.GetByIdAsync(request.OrganizationId, cancellationToken)
    //        ?? throw new NotFoundException(nameof(Organization), request.OrganizationId);

    //    if (org.IsVerified)
    //        throw new BadRequestException("Organization is already verified.");

    //    org.IsVerified = true;
    //    org.UpdatedAt = DateTime.UtcNow;
    //    _orgRepo.Update(org);

    //    // Activate the linked user account so they can log in
    //    var user = await _userRepo.GetByIdAsync(org.UserId, cancellationToken)
    //        ?? throw new NotFoundException(nameof(User), org.UserId);

    //    user.IsActive = true;
    //    user.UpdatedAt = DateTime.UtcNow;
    //    _userRepo.Update(user);

    //    await _uow.SaveChangesAsync(cancellationToken);

    //    return new VerifyOrganizationResponse(
    //        OrganizationId: org.Id,
    //        OrganizationName: org.Name,
    //        Email: user.Email,
    //        IsVerified: org.IsVerified,
    //        IsActive: user.IsActive,
    //        VerifiedAt: org.UpdatedAt!.Value);
    //}

    public async Task<VerifyOrganizationResponse> Handle(VerifyOrganizationCommand request, CancellationToken cancellationToken)
    {
        var org = await _orgRepo.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Organization), request.OrganizationId);

        if (org.Status == request.Status)
            throw new BadRequestException($"Organization is already {request.Status}.");

        var user = await _userRepo.GetByIdAsync(org.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), org.UserId);

        switch (request.Status)
        {
            case OrganizationStatus.Verified:
                org.Status = OrganizationStatus.Verified;
                user.IsActive = true;
                break;

            case OrganizationStatus.Rejected:
                org.Status = OrganizationStatus.Rejected;
                org.Reason = request.Reason;
                user.IsActive = false;
                break;

            case OrganizationStatus.Suspended:
                org.Status = OrganizationStatus.Suspended;
                org.Reason = request.Reason;
                user.IsActive = false;
                break;

            default:
                throw new BadRequestException($"Cannot manually set status to '{request.Status}'.");
        }

        org.UpdatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        _orgRepo.Update(org);
        _userRepo.Update(user);
        await _uow.SaveChangesAsync(cancellationToken);

        return new VerifyOrganizationResponse(
            OrganizationId: org.Id,
            OrganizationName: org.Name,
            Email: user.Email,
            Status: org.Status,
            IsActive: user.IsActive,
            UpdatedAt: org.UpdatedAt!.Value);
    }
}

