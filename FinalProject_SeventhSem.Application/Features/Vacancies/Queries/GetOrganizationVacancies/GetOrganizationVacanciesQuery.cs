using FinalProject_SeventhSem.Application.Common;
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

namespace FinalProject_SeventhSem.Application.Features.Vacancies.Queries.GetOrganizationVacancies;

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Returns all vacancies (published and draft) belonging to the requesting organization.
/// </summary>
public record GetOrganizationVacanciesQuery(int UserId)
    : IRequest<IReadOnlyList<VacancyResponse>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public class GetOrganizationVacanciesQueryHandler
    : IRequestHandler<GetOrganizationVacanciesQuery, IReadOnlyList<VacancyResponse>>
{
    private readonly IRepository<Vacancy> _vacancyRepo;
    private readonly IRepository<Organization> _orgRepo;

    public GetOrganizationVacanciesQueryHandler(IRepository<Vacancy> vacancyRepo, IRepository<Organization> orgRepo)
    {
        _vacancyRepo = vacancyRepo;
        _orgRepo = orgRepo;
    }

    public async Task<IReadOnlyList<VacancyResponse>> Handle(
        GetOrganizationVacanciesQuery request, CancellationToken cancellationToken)
    {
        var org = await OrganizationResolver.ResolveAsync(
            request.UserId, _orgRepo, cancellationToken);

        var all = await _vacancyRepo.GetAllAsync(
        include: q => q
            .Where(v => v.OrganizationId == org.Id)   // ← only this org's vacancies
            .Include(v => v.Organization)
            .Include(v => v.VacancySkills)
                .ThenInclude(vs => vs.Skill),
        cancellationToken);

        return all
            .Where(v => v.OrganizationId == org.Id)           // ← use org.Id not UserId
            .OrderByDescending(v => v.CreatedAt)
            .Select(v =>
            {
                bool isDeadlinePassed = DateTime.UtcNow > v.ApplicationDeadline;
                int daysRemaining = (int)Math.Ceiling(
                    (v.ApplicationDeadline - DateTime.UtcNow).TotalDays);

                return new VacancyResponse(
                    VacancyId: v.Id,
                    OrganizationId: v.OrganizationId,
                    OrganizationName: v.Organization.Name,
                    Title: v.Title,
                    Description: v.Description,
                    IsPublished: v.IsPublished,
                    PublishedAt: v.PublishedAt,
                    ApplicationDeadline: v.ApplicationDeadline,   // ← add
                    IsDeadlinePassed: isDeadlinePassed,        // ← add
                    DaysRemaining: daysRemaining,           // ← add
                    RequiredEducationLevel: v.RequiredEducationLevel?.ToString(),
                    RequiredFieldOfStudy: v.RequiredFieldOfStudy,
                    RequiredSkills: v.VacancySkills
                        .Where(vs => vs.IsRequired)
                        .Select(vs => new VacancySkillDto(vs.SkillId, vs.Skill.Name))
                        .ToList(),
                    OptionalSkills: v.VacancySkills
                        .Where(vs => !vs.IsRequired)
                        .Select(vs => new VacancySkillDto(vs.SkillId, vs.Skill.Name))
                        .ToList());
            })
            .ToList();
    }
}
