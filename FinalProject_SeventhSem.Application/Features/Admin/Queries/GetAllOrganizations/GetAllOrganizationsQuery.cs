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

namespace FinalProject_SeventhSem.Application.Features.Admin.Queries.GetAllOrganizations;

public record OrganizationSummaryDto(
    int OrganizationId,
    int UserId,
    string Name,
    string Email,
    string? WebsiteUrl,
    string? Reason,
    OrganizationStatus Status,
    DateTime CreatedAt
);


public record GetAllOrganizationsQuery(bool PendingOnly = false)
    : IRequest<IReadOnlyList<OrganizationSummaryDto>>;


public class GetAllOrganizationsQueryHandler
    : IRequestHandler<GetAllOrganizationsQuery, IReadOnlyList<OrganizationSummaryDto>>
{
    private readonly IRepository<Organization> _orgRepo;

    public GetAllOrganizationsQueryHandler(IRepository<Organization> orgRepo)
        => _orgRepo = orgRepo;

    public async Task<IReadOnlyList<OrganizationSummaryDto>> Handle(
        GetAllOrganizationsQuery request, CancellationToken cancellationToken)
    {
        var orgs = await _orgRepo.GetAllAsync(
    include: q => q.Include(o => o.User),
    cancellationToken: cancellationToken);

        var filtered = request.PendingOnly
            ? orgs.Where(o => o.Status != OrganizationStatus.Verified)
            : orgs;

        return filtered
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrganizationSummaryDto(
                OrganizationId: o.Id,
                UserId: o.UserId,
                Name: o.Name,
                Email: o.User.Email,
                WebsiteUrl: o.WebsiteUrl,
                Reason: o.Reason,
                Status: o.Status,
                CreatedAt: o.CreatedAt))
            .ToList();
    }
}

