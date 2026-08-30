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

namespace FinalProject_SeventhSem.Application.Features.Students.Queries.GetStudentProfile;

public class GetStudentProfileQueryHandler
    : IRequestHandler<GetStudentProfileQuery, StudentProfileResponse>
{
    private readonly IRepository<Student> _studentRepo;

    public GetStudentProfileQueryHandler(IRepository<Student> studentRepo)
        => _studentRepo = studentRepo;

    public async Task<StudentProfileResponse> Handle(
        GetStudentProfileQuery request,
        CancellationToken cancellationToken)
    {
        //var student = await StudentResolver.ResolveAsync(request.UserId, _studentRepo, cancellationToken);

        var students = await _studentRepo.GetAllAsync(
        include: q => q
            .Where(s => s.UserId == request.UserId)
            .Include(s => s.User)
            .Include(s => s.StudentSkills)
                .ThenInclude(ss => ss.Skill),
        cancellationToken);

        var student = students.FirstOrDefault()
            ?? throw new NotFoundException(nameof(Student), request.UserId);

        return new StudentProfileResponse(
            StudentId: student.Id,
            UserId: student.UserId,
            FullName: student.FullName,
            Email: student.User.Email,
            PhotoUrl: student.PhotoUrl,
            PhoneNumber: student.PhoneNumber,
            Bio: student.Bio,
            Nationality: student.Nationality,
            Location: student.Location,
            EducationLevel: student.EducationLevel?.ToString(),
            FieldOfStudy: student.FieldOfStudy,
            GitHubUrl: student.GitHubUrl,
            PortfolioUrl: student.PortfolioUrl,
            LinkedInUrl: student.LinkedInUrl,
            ResumeUrl: student.ResumeUrl,
            ConfirmedSkills: student.StudentSkills
                .Select(ss => new SkillSummary(ss.SkillId, ss.Skill.Name))
                .ToList());
    }
}

