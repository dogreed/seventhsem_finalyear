using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Models.Applications;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Applications.Queries.GetOrganizationApplications;

public record GetOrganizationApplicationsQuery(int UserId) : IRequest<IReadOnlyList<OrganizationApplicationResponse>>;

public class GetOrganizationApplicationsQueryHandler
    : IRequestHandler<GetOrganizationApplicationsQuery, IReadOnlyList<OrganizationApplicationResponse>>
{
    private readonly IRepository<Organization> _orgRepo;
    private readonly IRepository<FinalProject_SeventhSem.Domain.Entities.Application> _applicationRepo;

    public GetOrganizationApplicationsQueryHandler(
        IRepository<Organization> orgRepo,
        IRepository<FinalProject_SeventhSem.Domain.Entities.Application> applicationRepo)
    {
        _orgRepo = orgRepo;
        _applicationRepo = applicationRepo;
    }

    public async Task<IReadOnlyList<OrganizationApplicationResponse>> Handle(
        GetOrganizationApplicationsQuery request, CancellationToken ct)
    {

        var organization = await _orgRepo.GetAsync(
        o => o.UserId == request.UserId,   
        cancellationToken: ct)
        ?? throw new NotFoundException(nameof(Organization), request.UserId);

        var applications = await _applicationRepo.GetAllAsync(
            q => q
                .Include(a => a.Student)
                    .ThenInclude(s => s.Tests)  
                .Include(a => a.Vacancy)
                .Where(a => a.Vacancy.OrganizationId == organization.Id),
            ct);

        return applications
           .OrderByDescending(a => a.AppliedAt)
           .Select(a => new OrganizationApplicationResponse(
               ApplicationId: a.Id,
               VacancyId: a.VacancyId,
               VacancyTitle: a.Vacancy.Title,
               StudentId: a.StudentId,
               StudentName: a.Student.FullName,
               Status: a.Status.ToString(),
               AppliedAt: a.AppliedAt,
               ResumeUrl: a.Student.ResumeUrl,
               TestCount: a.Student.Tests.Count))
           .ToList();
    }
}

public record GetApplicationsByVacancyQuery(int VacancyId, int UserId) : IRequest<IReadOnlyList<OrganizationApplicationResponse>>;

public class GetApplicationsByVacancyQueryHandler
    : IRequestHandler<GetApplicationsByVacancyQuery, IReadOnlyList<OrganizationApplicationResponse>>
{
    private readonly IRepository<Organization> _orgRepo;
    private readonly IRepository<Vacancy> _vacancyRepo;
    private readonly IRepository<FinalProject_SeventhSem.Domain.Entities.Application> _applicationRepo;

    public GetApplicationsByVacancyQueryHandler(
        IRepository<Organization> orgRepo,
        IRepository<Vacancy> vacancyRepo,
        IRepository<FinalProject_SeventhSem.Domain.Entities.Application> applicationRepo)
    {
        _orgRepo = orgRepo;
        _vacancyRepo = vacancyRepo;
        _applicationRepo = applicationRepo;
    }

    public async Task<IReadOnlyList<OrganizationApplicationResponse>> Handle(
        GetApplicationsByVacancyQuery request, CancellationToken ct)
    {
      

        var organization = await _orgRepo.GetAsync(
        o => o.UserId == request.UserId,  
        cancellationToken: ct)
        ?? throw new NotFoundException(nameof(Organization), request.UserId);


        var vacancy = await _vacancyRepo.GetByIdAsync(request.VacancyId, ct)
            ?? throw new NotFoundException(nameof(Vacancy), request.VacancyId);

        if (vacancy.OrganizationId != organization.Id)
            throw new UnauthorizedException("You do not own this vacancy.");


        var applications = await _applicationRepo.GetAllAsync(
        q => q
            .Include(a => a.Student)
                .ThenInclude(s => s.Tests) 
            .Include(a => a.Vacancy)
            .Where(a => a.VacancyId == request.VacancyId),
        ct);

        return applications
            .OrderByDescending(a => a.AppliedAt)
            .Select(a => new OrganizationApplicationResponse(
                ApplicationId: a.Id,
                VacancyId: a.VacancyId,
                VacancyTitle: a.Vacancy.Title,
                StudentId: a.StudentId,
                StudentName: a.Student.FullName,
                Status: a.Status.ToString(),
                AppliedAt: a.AppliedAt,
                ResumeUrl: a.Student.ResumeUrl,
                TestCount: a.Student.Tests.Count))
            .ToList();
    }
}
