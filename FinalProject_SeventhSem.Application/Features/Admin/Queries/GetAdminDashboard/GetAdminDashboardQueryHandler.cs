using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Enums;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Admin.Queries.GetAdminDashboard;


public record AdminDashboardSummaryResponse(
    int TotalStudents,
    int VerifiedOrganizations,
    int TotalVacancies,
    int TotalApplications,
    int StudentsAttemptedTest
);
public record GetAdminDashboardSummaryQuery : IRequest<AdminDashboardSummaryResponse>;

public class GetAdminDashboardSummaryQueryHandler
    : IRequestHandler<GetAdminDashboardSummaryQuery, AdminDashboardSummaryResponse>
{
    private readonly IRepository<Student> _studentRepo;
    private readonly IRepository<Organization> _organizationRepo;
    private readonly IRepository<Vacancy> _vacancyRepo;
    private readonly IRepository<FinalProject_SeventhSem.Domain.Entities.Application> _applicationRepo;
    private readonly IRepository<Test> _testRepo;
    public GetAdminDashboardSummaryQueryHandler(
        IRepository<Student> studentRepo,
        IRepository<Organization> organizationRepo,
        IRepository<Vacancy> vacancyRepo,
        IRepository<Test> testRepo,
        IRepository<FinalProject_SeventhSem.Domain.Entities.Application> applicationRepo)
    {
        _studentRepo = studentRepo;
        _organizationRepo = organizationRepo;
        _vacancyRepo = vacancyRepo;
        _applicationRepo = applicationRepo;
        _testRepo = testRepo;
    }

    public async Task<AdminDashboardSummaryResponse> Handle(
    GetAdminDashboardSummaryQuery request,
    CancellationToken cancellationToken)
    {
        
        var studentCount = await _studentRepo.CountAsync(cancellationToken);

        var verifiedOrgCount = await _organizationRepo.CountAsync(
            predicate: o => o.Status == OrganizationStatus.Verified,
            cancellationToken: cancellationToken);

        var vacancyCount = await _vacancyRepo.CountAsync(cancellationToken);

        var applicationCount = await _applicationRepo.CountAsync(cancellationToken);

        var studentsAttemptedTest = await _testRepo.CountAsync(
           predicate: t => t.StudentId != 0,  
           cancellationToken: cancellationToken);
       
        return new AdminDashboardSummaryResponse(
            TotalStudents: studentCount,
            VerifiedOrganizations: verifiedOrgCount,
            TotalVacancies: vacancyCount,
            TotalApplications: applicationCount,
            StudentsAttemptedTest: studentsAttemptedTest
        );
    }
}
