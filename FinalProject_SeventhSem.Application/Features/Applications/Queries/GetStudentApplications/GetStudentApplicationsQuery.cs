using FinalProject_SeventhSem.Application.Common;
using FinalProject_SeventhSem.Application.Models.Applications;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Applications.Queries.GetStudentApplications;

public record GetStudentApplicationsQuery(int UserId)
    : IRequest<IReadOnlyList<ApplicationResponse>>;


public class GetStudentApplicationsQueryHandler
    : IRequestHandler<GetStudentApplicationsQuery, IReadOnlyList<ApplicationResponse>>
{
    private readonly IRepository<Student> _studentRepo;
    private readonly IRepository<FinalProject_SeventhSem.Domain.Entities.Application> _applicationRepo;
    private readonly IRepository<Skill> _skillRepo;

    public GetStudentApplicationsQueryHandler(
        IRepository<Student> studentRepo,
        IRepository<FinalProject_SeventhSem.Domain.Entities.Application> applicationRepo,
        IRepository<Skill> skillRepo)
    {
        _studentRepo = studentRepo;
        _applicationRepo = applicationRepo;
        _skillRepo = skillRepo;
    }

    public async Task<IReadOnlyList<ApplicationResponse>> Handle(
        GetStudentApplicationsQuery request, CancellationToken cancellationToken)
    {
        var student = await StudentResolver.ResolveAsync(request.UserId, _studentRepo, cancellationToken);
        var allSkills = await _skillRepo.GetAllAsync(cancellationToken);
        var skillMap = allSkills.ToDictionary(s => s.Id, s => s.Name);



        var apps = await _applicationRepo.GetAllAsync(
    q => q
        .Include(a => a.Vacancy)
        .Include(a => a.Student)
        .Include(a => a.MatchSnapshot)
        .Where(a => a.StudentId == student.Id)
        .OrderByDescending(a => a.AppliedAt),
    cancellationToken);

        return apps.Select(a =>
        {
            ApplicationSnapshotDto? snapshot = null;
            if (a.MatchSnapshot is not null)
            {
                var missingIds = JsonSerializer
                    .Deserialize<List<int>>(a.MatchSnapshot.MissingSkillIdsJson) ?? [];
                snapshot = new ApplicationSnapshotDto(
                    RequirementFit: a.MatchSnapshot.RequirementFit,
                    OptionalFit: a.MatchSnapshot.OptionalFit,
                    EducationBonus: a.MatchSnapshot.EducationBonus,
                    MissingSkills: missingIds.Select(id => skillMap.GetValueOrDefault(id, "Unknown")).ToList(),
                    ComputedAt: a.MatchSnapshot.ComputedAt);
            }

            return new ApplicationResponse(
                ApplicationId: a.Id,
                VacancyId: a.VacancyId,
                VacancyTitle: a.Vacancy.Title,
                StudentId: a.StudentId,
                StudentName: a.Student.FullName,
                Status: a.Status.ToString(),
                AppliedAt: a.AppliedAt,
                StatusUpdatedAt: a.StatusUpdatedAt,
                MatchSnapshot: snapshot);
        }).ToList();
    }
}

