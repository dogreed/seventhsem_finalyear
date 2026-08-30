using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Features.Vacancies.Commands.PublishVacancy;
using FinalProject_SeventhSem.Application.Models.Vacancies;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Vacancies.Queries.GetVacancy;

public class GetVacancyQueryHandler: IRequestHandler<GetVacancyQuery, IReadOnlyList<VacancyResponse>>
{
    private readonly IRepository<Vacancy> _vacancyRepo;

    public GetVacancyQueryHandler(IRepository<Vacancy> vacancyRepo)
        => _vacancyRepo = vacancyRepo;
    public async Task<IReadOnlyList<VacancyResponse>> Handle(GetVacancyQuery request, CancellationToken ct = default )
    {
        var vacancies = await _vacancyRepo.GetAllAsync(
            include: q => q.Include(v => v.Organization)
                           .Include(v => v.VacancySkills)
                               .ThenInclude(vs => vs.Skill),
            cancellationToken: ct);

        // 2. Map the list of entities to the list of Response DTOs
        var response = vacancies.Select(v =>
        {
            bool isDeadlinePassed = DateTime.UtcNow > v.ApplicationDeadline;
            int daysRemaining = (int)Math.Ceiling((v.ApplicationDeadline - DateTime.UtcNow).TotalDays);

            return new VacancyResponse(
                 VacancyId: v.Id,
                 OrganizationId: v.OrganizationId,
                 OrganizationName: v.Organization?.Name ?? "Unknown Organization", // Defensive coding check
                 Title: v.Title,
                 Description: v.Description,
                 IsPublished: v.IsPublished,
                 PublishedAt: v.PublishedAt,
                 ApplicationDeadline: v.ApplicationDeadline,
                 IsDeadlinePassed: isDeadlinePassed,
                 DaysRemaining: daysRemaining,
                 RequiredEducationLevel: v.RequiredEducationLevel?.ToString(),
                 RequiredFieldOfStudy: v.RequiredFieldOfStudy,
                 RequiredSkills: v.VacancySkills
                     .Where(vs => vs.IsRequired)
                     .Select(vs => new VacancySkillDto(vs.SkillId, vs.Skill.Name))
                     .ToList(),
                 OptionalSkills: v.VacancySkills
                     .Where(vs => !vs.IsRequired)
                     .Select(vs => new VacancySkillDto(vs.SkillId, vs.Skill.Name))
                     .ToList()
            );
        }).ToList();

        return response;
    }
}
