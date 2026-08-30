using FinalProject_SeventhSem.Application.Common;
using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Interfaces;
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

namespace FinalProject_SeventhSem.Application.Features.Applications.Commands.ApplyToVacancy;

public class ApplyToVacancyCommandHandler : IRequestHandler<ApplyToVacancyCommand, ApplicationResponse>
{
    private readonly IRepository<Student> _studentRepo;
    private readonly IRepository<Vacancy> _vacancyRepo;
    private readonly IRepository<FinalProject_SeventhSem.Domain.Entities.Application> _applicationRepo;
    private readonly IRepository<ApplicationMatchSnapshot> _snapshotRepo;
    private readonly IRepository<Skill> _skillRepo;
    private readonly IMatchingService _matching;
    private readonly IUnitOfWork _uow;

    public ApplyToVacancyCommandHandler(
        IRepository<Student> studentRepo,
        IRepository<Vacancy> vacancyRepo,
        IRepository<FinalProject_SeventhSem.Domain.Entities.Application> applicationRepo,
        IRepository<ApplicationMatchSnapshot> snapshotRepo,
        IRepository<Skill> skillRepo,
        IMatchingService matching,
        IUnitOfWork uow)
    {
        _studentRepo = studentRepo;
        _vacancyRepo = vacancyRepo;
        _applicationRepo = applicationRepo;
        _snapshotRepo = snapshotRepo;
        _skillRepo = skillRepo;
        _matching = matching;
        _uow = uow;
    }

    public async Task<ApplicationResponse> Handle(
        ApplyToVacancyCommand request, CancellationToken cancellationToken)
    {


        var student = await _studentRepo.GetAsync(
            predicate: s => s.UserId == request.StudentId,
            include: q => q.Include(s => s.StudentSkills),
            cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(Student), request.StudentId);



        var vacancy = await _vacancyRepo.GetByIdAsync(
                id: request.VacancyId,
                include: q => q
                    .Include(v => v.VacancySkills)
                        .ThenInclude(vs => vs.Skill)
                    .Include(v => v.Organization),
                cancellationToken: cancellationToken)
                ?? throw new NotFoundException(nameof(Vacancy), request.VacancyId);

        if (!vacancy.IsPublished)
            throw new BadRequestException("Cannot apply to an unpublished vacancy.");

        if (DateTime.UtcNow > vacancy.ApplicationDeadline)
            throw new BadRequestException(
                $"The application deadline was {vacancy.ApplicationDeadline:dd MMM yyyy HH:mm} UTC. " +
                $"This vacancy is no longer accepting applications.");

       
        var existing = await _applicationRepo.GetAsync(
            predicate: a => a.StudentId == student.Id && a.VacancyId == vacancy.Id,
            cancellationToken: cancellationToken);

        if (existing is not null)
        {
            throw new BadRequestException("You have already applied to this vacancy.");
        }


       
        var matchResult = _matching.Match(student, vacancy);

        var application = new FinalProject_SeventhSem.Domain.Entities.Application
        {
            StudentId = student.Id,
            VacancyId = vacancy.Id,
            AppliedAt = DateTime.UtcNow
        };
        await _applicationRepo.AddAsync(application, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        // Resolve missing skill names for snapshot
        //var allSkills = await _skillRepo.GetAllAsync(cancellationToken);
        //var skillMap = allSkills.ToDictionary(s => s.Id, s => s.Name);

        //var missingIds = _matching.ComputeSkillGap(
        //    student.StudentSkills.Select(ss => ss.SkillId),
        //    vacancy.VacancySkills.Where(vs => vs.IsRequired).Select(vs => vs.SkillId),
        //    vacancy.VacancySkills.Where(vs => !vs.IsRequired).Select(vs => vs.SkillId));

        var snapshot = new ApplicationMatchSnapshot
        {
            ApplicationId = application.Id,
            RequirementFit = matchResult.RequirementFit,
            OptionalFit = matchResult.OptionalFit,
            EducationBonus = matchResult.EducationBonus,
            MissingSkillIdsJson = JsonSerializer.Serialize(matchResult.MissingSkillIds),
            ComputedAt = DateTime.UtcNow
        };
        await _snapshotRepo.AddAsync(snapshot, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new ApplicationResponse(
            ApplicationId: application.Id,
            VacancyId: vacancy.Id,
            VacancyTitle: vacancy.Title,
            StudentId: student.Id,
            StudentName: student.FullName,
            Status: application.Status.ToString(),
            AppliedAt: application.AppliedAt,
            StatusUpdatedAt: null,
            MatchSnapshot: new ApplicationSnapshotDto(
                RequirementFit: snapshot.RequirementFit,
                OptionalFit: snapshot.OptionalFit,
                EducationBonus: snapshot.EducationBonus,
                MissingSkills: matchResult.MissingSkills,
                ComputedAt: snapshot.ComputedAt));
    }
}

