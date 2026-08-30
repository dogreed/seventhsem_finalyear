using FinalProject_SeventhSem.Application.Exceptions;
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

namespace FinalProject_SeventhSem.Application.Features.Vacancies.Queries.GetVacancyById;

public class GetVacancyByIdQueryHandler : IRequestHandler<GetVacancyByIdQuery, VacancyResponse>
{
    private readonly IRepository<Vacancy> _vacancyRepo;

    public GetVacancyByIdQueryHandler(IRepository<Vacancy> vacancyRepo)
        => _vacancyRepo = vacancyRepo;

    public async Task<VacancyResponse> Handle(
        GetVacancyByIdQuery request, CancellationToken cancellationToken)
    {
        var v = await _vacancyRepo.GetByIdAsync(
               id: request.VacancyId,
               include: q => q.Include(v => v.Organization)
                              .Include(v => v.VacancySkills)
                                  .ThenInclude(vs => vs.Skill),
               cancellationToken: cancellationToken)
               ?? throw new NotFoundException(nameof(Vacancy), request.VacancyId);
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
    }
}


