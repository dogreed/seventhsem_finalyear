using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Interfaces;
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

namespace FinalProject_SeventhSem.Application.Features.Vacancies.Queries.GetVacancyMatches;

public class GetVacancyMatchesQueryHandler
    : IRequestHandler<GetVacancyMatchesQuery, IReadOnlyList<VacancyMatchResult>>
{
    private readonly IRepository<Student> _studentRepo;
    private readonly IRepository<Vacancy> _vacancyRepo;
    private readonly IMatchingService _matching;

    public GetVacancyMatchesQueryHandler(
        IRepository<Student> studentRepo,
        IRepository<Vacancy> vacancyRepo,
        IMatchingService matching)
    {
        _studentRepo = studentRepo;
        _vacancyRepo = vacancyRepo;
        _matching = matching;
    }

    public async Task<IReadOnlyList<VacancyMatchResult>> Handle(
        GetVacancyMatchesQuery request, CancellationToken cancellationToken)
    {

        var student = await _studentRepo.GetAsync(
    predicate: s => s.UserId == request.StudentId,
    include: q => q.Include(s => s.StudentSkills),
    cancellationToken: cancellationToken)
    ?? throw new NotFoundException(nameof(Student), request.StudentId);



        var vacancies = (await _vacancyRepo.GetAllAsync(
                include: q => q
                    .Include(v => v.VacancySkills)
                        .ThenInclude(vs => vs.Skill)
                    .Include(v => v.Organization),
                cancellationToken: cancellationToken))
                .Where(v => v.IsPublished && DateTime.UtcNow <= v.ApplicationDeadline)
                .ToList();

        var results = vacancies
            .Select(v => _matching.Match(student, v))
            //.Where(r=> r.IsEligible)
            .Where( r=>r.RequirementFit >= 60)
            .OrderByDescending(r => r.RequirementFit)
            .ThenByDescending(r => r.OptionalFit)
            .ThenByDescending(r => r.EducationBonus)
            .ToList();

        return results;
    }
}

