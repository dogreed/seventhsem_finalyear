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

namespace FinalProject_SeventhSem.Application.Features.Admin.Queries.GetAllStudents;

public record StudentSummaryDto(
    int StudentId,
    int UserId,
    string FullName,
    string Email,
    EducationLevel? EducationLevel,
    string? FieldOfStudy,
    IReadOnlyList<string> Skills,
    int TotalApplications,
    int TotalTests
);

public record GetAllStudentsQuery : IRequest<IReadOnlyList<StudentSummaryDto>>;

public class GetAllStudentsQueryHandler
    : IRequestHandler<GetAllStudentsQuery, IReadOnlyList<StudentSummaryDto>>
{
    private readonly IRepository<Student> _studentRepo;

    public GetAllStudentsQueryHandler(IRepository<Student> studentRepo)
        => _studentRepo = studentRepo;

    public async Task<IReadOnlyList<StudentSummaryDto>> Handle(
        GetAllStudentsQuery request, CancellationToken cancellationToken)
    {
        var students = await _studentRepo.GetAllAsync(
            include: q => q
                .Include(s => s.User)
                .Include(s => s.StudentSkills)
                    .ThenInclude(ss => ss.Skill)   // adjust if your nav prop differs
                .Include(s => s.Applications)
                .Include(s => s.Tests),
            cancellationToken: cancellationToken);

        return students
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new StudentSummaryDto(
                StudentId: s.Id,
                UserId: s.UserId,
                FullName: s.FullName,
                Email: s.User.Email,
                EducationLevel: s.EducationLevel,
                FieldOfStudy: s.FieldOfStudy,
                Skills: s.StudentSkills
                                       .Select(ss => ss.Skill.Name)  // adjust nav prop if needed
                                       .ToList(),
                TotalApplications: s.Applications.Count,
                TotalTests: s.Tests.Count))
            .ToList();
    }
}