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

namespace FinalProject_SeventhSem.Application.Features.Admin.Queries.GetOrganizationDashboard;

public record OrganizationDashboardResponse(
    string Name,
    string? WebsiteUrl,
    string? LogoUrl,
    int TotalVacanciesPosted,
    int TotalApplicationsReceived,
    int ApplicationsOffered,
    int ApplicationsRejected
);

public record GetOrganizationDashboardQuery : IRequest<OrganizationDashboardResponse>;


public class GetOrganizationDashboardQueryHandler
    : IRequestHandler<GetOrganizationDashboardQuery, OrganizationDashboardResponse>
{
    private readonly IRepository<Organization> _organizationRepo;
    private readonly IRepository<Vacancy> _vacancyRepo;
    private readonly IRepository<FinalProject_SeventhSem.Domain.Entities.Application> _applicationRepo;
    private readonly ICurrentUserService _currentUserService; // inject to get logged-in org's userId

    public GetOrganizationDashboardQueryHandler(
        IRepository<Organization> organizationRepo,
        IRepository<Vacancy> vacancyRepo,
        IRepository<FinalProject_SeventhSem.Domain.Entities.Application> applicationRepo,
        ICurrentUserService currentUserService)
    {
        _organizationRepo = organizationRepo;
        _vacancyRepo = vacancyRepo;
        _applicationRepo = applicationRepo;
        _currentUserService = currentUserService;
    }

    public async Task<OrganizationDashboardResponse> Handle(
        GetOrganizationDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var organization = await _organizationRepo.GetAsync(
        predicate: o => o.UserId == userId,
        cancellationToken: cancellationToken)
        ?? throw new NotFoundException(nameof(Organization), userId);

        var vacancies = await _vacancyRepo.GetAllAsync(
            q => q.Where(v => v.OrganizationId == organization.Id),
            cancellationToken);

        var vacancyIds = vacancies.Select(v => v.Id).ToHashSet();
        var totalVacancies = vacancyIds.Count;

        if (totalVacancies == 0)
        {
            return new OrganizationDashboardResponse(
                Name: organization.Name,
                WebsiteUrl: organization.WebsiteUrl,
                LogoUrl: organization.LogoUrl,
                TotalVacanciesPosted: 0,
                TotalApplicationsReceived: 0,
                ApplicationsOffered: 0,
                ApplicationsRejected: 0);
        }

        var totalApplications = await _applicationRepo.CountAsync(
            predicate: a => vacancyIds.Contains(a.VacancyId),
            cancellationToken: cancellationToken);

        var offeredApplications = await _applicationRepo.CountAsync(
            predicate: a => vacancyIds.Contains(a.VacancyId)
                         && a.Status == ApplicationStatus.Offered,
            cancellationToken: cancellationToken);

        var rejectedApplications = await _applicationRepo.CountAsync(
            predicate: a => vacancyIds.Contains(a.VacancyId)
                         && a.Status == ApplicationStatus.Rejected,
            cancellationToken: cancellationToken);

        return new OrganizationDashboardResponse(
            Name: organization.Name,
            WebsiteUrl: organization.WebsiteUrl,
            LogoUrl: organization.LogoUrl,
            TotalVacanciesPosted: totalVacancies,
            TotalApplicationsReceived: totalApplications,
            ApplicationsOffered: offeredApplications,
            ApplicationsRejected: rejectedApplications);
    }
}
