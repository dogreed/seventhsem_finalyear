using FinalProject_SeventhSem.Application.Common;
using FinalProject_SeventhSem.Application.Models.Vacancies;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Vacancies.Commands.CreateVacancy;

public class CreateVacancyCommandHandler : IRequestHandler<CreateVacancyCommand, VacancyResponse>
{
    private readonly IRepository<Vacancy> _vacancyRepo;
    private readonly IRepository<VacancySkill> _vacancySkillRepo;
    private readonly IRepository<Skill> _skillRepo;
    private readonly IRepository<Organization> _orgRepo;
    private readonly IUnitOfWork _uow;

    public CreateVacancyCommandHandler(
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
        CreateVacancyCommand request, CancellationToken cancellationToken)
    {
        var org = await OrganizationResolver.ResolveAsync(
            request.OrganizationId,   // this is actually UserId from JWT
            _orgRepo,
            cancellationToken);

        var vacancy = new Vacancy
        {
            OrganizationId = org.Id,
            Title = request.Title,
            Description = request.Description,
            ApplicationDeadline = request.ApplicationDeadline.ToUniversalTime(),
            RequiredEducationLevel = request.RequiredEducationLevel,
            RequiredFieldOfStudy = request.RequiredFieldOfStudy,
            IsPublished = true,                      // ← change this
            PublishedAt = DateTime.UtcNow
        };

        await _vacancyRepo.AddAsync(vacancy, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);


        var allSkills = await _skillRepo.GetAllAsync(cancellationToken);
        var skillMap = allSkills.ToDictionary(s => s.Id, s => s.Name);


        foreach (var skillId in request.RequiredSkillIds.Distinct().Where(skillMap.ContainsKey))
            await _vacancySkillRepo.AddAsync(
                new VacancySkill { VacancyId = vacancy.Id, SkillId = skillId, IsRequired = true },
                cancellationToken);

        // Add optional skills — skip nulls, invalid IDs, and duplicates from required
        if (request.OptionalSkillIds is { Count: > 0 })
        {
            foreach (var skillId in request.OptionalSkillIds.Distinct()
                         .Where(id => skillMap.ContainsKey(id)
                                   && !request.RequiredSkillIds.Contains(id)))
                await _vacancySkillRepo.AddAsync(
                    new VacancySkill { VacancyId = vacancy.Id, SkillId = skillId, IsRequired = false },
                    cancellationToken);
        }

        // Add required skills
        //foreach (var skillId in request.RequiredSkillIds.Distinct())
        //    await _vacancySkillRepo.AddAsync(
        //        new VacancySkill { VacancyId = vacancy.Id, SkillId = skillId, IsRequired = true },
        //        cancellationToken);

        //// Add optional skills (skip any already in required)
        //foreach (var skillId in request.OptionalSkillIds.Distinct()
        //             .Where(id => !request.RequiredSkillIds.Contains(id)))
        //    await _vacancySkillRepo.AddAsync(
        //        new VacancySkill { VacancyId = vacancy.Id, SkillId = skillId, IsRequired = false },
        //        cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);
        int daysRemaining = (int)Math.Ceiling(
        (vacancy.ApplicationDeadline - DateTime.UtcNow).TotalDays);

        // Build response
        //var allSkills = await _skillRepo.GetAllAsync(cancellationToken);
        //var skillMap = allSkills.ToDictionary(s => s.Id, s => s.Name);

        return new VacancyResponse(
            VacancyId: vacancy.Id,
            OrganizationId: vacancy.OrganizationId,
            OrganizationName: org.Name, // populated by query layer
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

