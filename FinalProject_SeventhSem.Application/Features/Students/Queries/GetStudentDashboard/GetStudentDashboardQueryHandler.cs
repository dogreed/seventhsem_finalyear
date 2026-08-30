using FinalProject_SeventhSem.Application.Common;
using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Models.Students;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Students.Queries.GetStudentDashboard;



/// <summary>
/// Computes profile completeness on the fly (Option A).
/// Max score = 100. No extra DB column required.
///
/// Scoring table:
///   FullName       5  |  Photo         5  |  Phone       5
///   Education     15  |  Skills     0-20  |  Resume     15
///   GitHub        10  |  Portfolio  10    |  LinkedIn    5
///   Bio            5  |  Nationality  5
/// </summary>
public class GetStudentDashboardQueryHandler
    : IRequestHandler<GetStudentDashboardQuery, ProfileCompletenessResponse>
{
    private readonly IRepository<Student> _studentRepo;
    private readonly IRepository<FinalProject_SeventhSem.Domain.Entities.Application> _applicationRepo; // Added
    private readonly IRepository<Test> _testRepo;

    public GetStudentDashboardQueryHandler(
            IRepository<Student> studentRepo,
            IRepository<FinalProject_SeventhSem.Domain.Entities.Application> applicationRepo,
            IRepository<Test> testRepo)
    {
        _studentRepo = studentRepo;
        _applicationRepo = applicationRepo;
        _testRepo = testRepo;
    }

    public async Task<ProfileCompletenessResponse> Handle(
        GetStudentDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var student = await _studentRepo.GetAsync(
                   predicate: s => s.UserId == request.UserId,
                   include: q => q.Include(s => s.StudentSkills),
                   cancellationToken: cancellationToken)
                   ?? throw new NotFoundException(
                       $"No student profile found for UserId {request.UserId}.");
        int totalApplications = await _applicationRepo.CountAsync(
       predicate: a => a.StudentId == student.Id,
       cancellationToken: cancellationToken);

        int totalTests = await _testRepo.CountAsync(
            predicate: t => t.StudentId == student.Id,
            cancellationToken: cancellationToken);



        bool hasFullName = !string.IsNullOrWhiteSpace(student.FullName);
        bool hasPhoto = !string.IsNullOrWhiteSpace(student.PhotoUrl);
        bool hasPhone = !string.IsNullOrWhiteSpace(student.PhoneNumber);
        bool hasEducation = student.EducationLevel.HasValue && !string.IsNullOrWhiteSpace(student.FieldOfStudy);
        bool hasResume = !string.IsNullOrWhiteSpace(student.ResumeUrl);
        bool hasGitHub = !string.IsNullOrWhiteSpace(student.GitHubUrl);
        bool hasPortfolio = !string.IsNullOrWhiteSpace(student.PortfolioUrl);
        bool hasLinkedIn = !string.IsNullOrWhiteSpace(student.LinkedInUrl);
        bool hasBio = !string.IsNullOrWhiteSpace(student.Bio);
        bool hasNationality = !string.IsNullOrWhiteSpace(student.Nationality);

        int skillCount = student.StudentSkills.Count;
        int skillPoints = skillCount >= 3 ? 20
                        : skillCount == 2 ? 10
                        : skillCount == 1 ? 5
                        : 0;

        int total = (hasFullName ? 5 : 0)
                  + (hasPhoto ? 5 : 0)
                  + (hasPhone ? 5 : 0)
                  + (hasEducation ? 15 : 0)
                  + skillPoints
                  + (hasResume ? 15 : 0)
                  + (hasGitHub ? 10 : 0)
                  + (hasPortfolio ? 10 : 0)
                  + (hasLinkedIn ? 5 : 0)
                  + (hasBio ? 5 : 0)
                  + (hasNationality ? 5 : 0);

        return new ProfileCompletenessResponse(
            TotalScore: total,
            HasFullName: hasFullName,
            HasPhoto: hasPhoto,
            HasPhoneNumber: hasPhone,
            HasEducation: hasEducation,
            SkillPoints: skillPoints,
            HasResume: hasResume,
            HasGitHub: hasGitHub,
            HasPortfolio: hasPortfolio,
            HasLinkedIn: hasLinkedIn,
            HasBio: hasBio,
            HasNationality: hasNationality,
            TotalApplicationApplied: totalApplications,
            TotalTestsAttempted: totalTests
        );
    }
}


