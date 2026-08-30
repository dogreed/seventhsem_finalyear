using FinalProject_SeventhSem.Application.Common;
using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Models.Vacancies;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Enums;
using FinalProject_SeventhSem.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Vacancies.Commands.UpdateVacancy;

public record UpdateVacancyCommand(
    int VacancyId,
    int OrganizationId,
    string Title,
    string Description,
     DateTime ApplicationDeadline,
    EducationLevel? RequiredEducationLevel,
    string? RequiredFieldOfStudy,
    IReadOnlyList<int> RequiredSkillIds,
    IReadOnlyList<int>? OptionalSkillIds
) : IRequest<VacancyResponse>;

// ── Validator ─────────────────────────────────────────────────────────────────

public class UpdateVacancyCommandValidator : AbstractValidator<UpdateVacancyCommand>
{
    public UpdateVacancyCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.ApplicationDeadline)
            .NotEmpty()
            .WithMessage("Application deadline is required.")
            .Must(d => d.ToUniversalTime() > DateTime.UtcNow)
            .WithMessage("Application deadline must be a future date.");
        RuleFor(x => x.RequiredSkillIds)
           .NotEmpty()
           .WithMessage("At least one required skill must be specified.")
           .Must(ids => ids.All(id => id > 0))
           .WithMessage("All skill IDs must be greater than 0.")
           .Must(ids => ids.Distinct().Count() == ids.Count)
           .WithMessage("Duplicate required skill IDs are not allowed.");

        RuleFor(x => x.OptionalSkillIds)
            .Must(ids => ids == null || ids.All(id => id > 0))
            .WithMessage("All optional skill IDs must be greater than 0.")
            .Must(ids => ids == null || ids.Distinct().Count() == ids.Count)
            .WithMessage("Duplicate optional skill IDs are not allowed.")
            .When(x => x.OptionalSkillIds != null && x.OptionalSkillIds.Any());
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public class UpdateVacancyCommandHandler : IRequestHandler<UpdateVacancyCommand, VacancyResponse>
{
    private readonly IRepository<Vacancy> _vacancyRepo;
    private readonly IRepository<VacancySkill> _vacancySkillRepo;
    private readonly IRepository<Skill> _skillRepo;
    private readonly IRepository<Organization> _orgRepo;
    private readonly IUnitOfWork _uow;

    public UpdateVacancyCommandHandler(
        IRepository<Vacancy> vacancyRepo,
        IRepository<VacancySkill> vacancySkillRepo,
        IRepository<Skill> skillRepo,
        IRepository<Organization> orgRepo,
        IUnitOfWork uow)
    {
        _vacancyRepo = vacancyRepo;
        _vacancySkillRepo = vacancySkillRepo;
        _skillRepo = skillRepo;
        _orgRepo = orgRepo;
        _uow = uow;
    }

    public async Task<VacancyResponse> Handle(
        UpdateVacancyCommand request, CancellationToken cancellationToken)
    {
       

        var vacancy = await _vacancyRepo.GetByIdAsync(
               id: request.VacancyId,
               include: q => q.Include(v => v.VacancySkills),
               cancellationToken: cancellationToken)
               ?? throw new NotFoundException(nameof(Vacancy), request.VacancyId);

        var org = await OrganizationResolver.ResolveAsync(
        request.OrganizationId, _orgRepo, cancellationToken);

        if (vacancy.OrganizationId != org.Id)
            throw new UnauthorizedException("You do not own this vacancy.");

       

        // Update scalar fields
        vacancy.Title = request.Title;
        vacancy.Description = request.Description;
        vacancy.ApplicationDeadline = request.ApplicationDeadline.ToUniversalTime();
        vacancy.RequiredEducationLevel = request.RequiredEducationLevel;
        vacancy.RequiredFieldOfStudy = request.RequiredFieldOfStudy;
        vacancy.UpdatedAt = DateTime.UtcNow;
        _vacancyRepo.Update(vacancy);

        // Replace skills: remove all existing, re-add from request
        var existingSkills = vacancy.VacancySkills.ToList();
        foreach (var vs in existingSkills)
            _vacancySkillRepo.Remove(vs);

        await _uow.SaveChangesAsync(cancellationToken);

        var allSkills = await _skillRepo.GetAllAsync(cancellationToken);
        var skillMap = allSkills.ToDictionary(s => s.Id, s => s.Name);

        // Required skills
        foreach (var skillId in request.RequiredSkillIds.Distinct()
                     .Where(skillMap.ContainsKey))
            await _vacancySkillRepo.AddAsync(
                new VacancySkill { VacancyId = vacancy.Id, SkillId = skillId, IsRequired = true },
                cancellationToken);

        // Optional skills — only if provided, skip duplicates of required
        if (request.OptionalSkillIds is { Count: > 0 })
        {
            foreach (var skillId in request.OptionalSkillIds.Distinct()
                         .Where(id => skillMap.ContainsKey(id)
                                   && !request.RequiredSkillIds.Contains(id)))
                await _vacancySkillRepo.AddAsync(
                    new VacancySkill { VacancyId = vacancy.Id, SkillId = skillId, IsRequired = false },
                    cancellationToken);
        }


      

        await _uow.SaveChangesAsync(cancellationToken);

        int daysRemaining = (int)Math.Ceiling(
         (vacancy.ApplicationDeadline - DateTime.UtcNow).TotalDays);

        return new VacancyResponse(
            VacancyId: vacancy.Id,
            OrganizationId: vacancy.OrganizationId,
            OrganizationName: org.Name,
            Title: vacancy.Title,
            Description: vacancy.Description,
            IsPublished: vacancy.IsPublished,
            PublishedAt: vacancy.PublishedAt,
             ApplicationDeadline: vacancy.ApplicationDeadline,
            IsDeadlinePassed: false,
            DaysRemaining: daysRemaining,
            RequiredEducationLevel: vacancy.RequiredEducationLevel?.ToString(),
            RequiredFieldOfStudy: vacancy.RequiredFieldOfStudy,
            RequiredSkills: request.RequiredSkillIds
            .Where(skillMap.ContainsKey)
            .Select(id => new VacancySkillDto(id, skillMap[id]))
            .ToList(),
        OptionalSkills: (request.OptionalSkillIds ?? [])
            .Where(skillMap.ContainsKey)
            .Select(id => new VacancySkillDto(id, skillMap[id]))
            .ToList());
    }
}
