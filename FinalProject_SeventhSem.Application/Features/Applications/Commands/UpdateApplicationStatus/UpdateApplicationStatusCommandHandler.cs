using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Applications.Commands.UpdateApplicationStatus;

public class UpdateApplicationStatusCommandHandler
    : IRequestHandler<UpdateApplicationStatusCommand>
{
    private readonly IRepository<FinalProject_SeventhSem.Domain.Entities.Application> _applicationRepo;
    private readonly IUnitOfWork _uow;
    private readonly IRepository<Organization> _organizationRepo;

    public UpdateApplicationStatusCommandHandler(
        IRepository<FinalProject_SeventhSem.Domain.Entities.Application> applicationRepo,
        IRepository<Organization> organizationRepo, IUnitOfWork uow)
    {
        _applicationRepo = applicationRepo;
        _organizationRepo = organizationRepo;
        _uow = uow;
    }

    public async Task Handle(
        UpdateApplicationStatusCommand request, CancellationToken cancellationToken)
    {
        var organization = await _organizationRepo.GetAsync(
             o => o.UserId == request.OrganizationId,
             cancellationToken: cancellationToken)
             ?? throw new NotFoundException(nameof(Organization), request.OrganizationId);

        var application = await _applicationRepo.GetByIdAsync(
            request.ApplicationId,
            include: q => q.Include(a => a.Vacancy),
            cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(Application), request.ApplicationId);

        // Now compare using the real Organization.Id
        if (application.Vacancy.OrganizationId != organization.Id)
            throw new UnauthorizedException("You do not own the vacancy for this application.");

        application.Status = request.NewStatus;
        application.StatusUpdatedAt = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;

        _applicationRepo.Update(application);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}


