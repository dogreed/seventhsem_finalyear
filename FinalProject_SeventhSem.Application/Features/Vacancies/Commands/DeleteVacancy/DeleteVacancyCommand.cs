using FinalProject_SeventhSem.Application.Common;
using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Vacancies.Commands.DeleteVacancy;

public record DeleteVacancyCommand(
    int VacancyId,
    int OrganizationId
) : IRequest;

public class DeleteVacancyCommandHandler : IRequestHandler<DeleteVacancyCommand>
{
    private readonly IRepository<Vacancy> _vacancyRepo;
    private readonly IRepository<VacancySkill> _vacancySkillRepo;
    private readonly IRepository<Organization> _orgRepo;
    private readonly IUnitOfWork _uow;

    public DeleteVacancyCommandHandler(
        IRepository<Vacancy> vacancyRepo,
        IRepository<VacancySkill> vacancySkillRepo,
        IRepository<Organization> orgRepo,
        IUnitOfWork uow)
    {
        _vacancyRepo = vacancyRepo;
        _vacancySkillRepo = vacancySkillRepo;
        _orgRepo = orgRepo;
        _uow = uow;
    }

    public async Task Handle(DeleteVacancyCommand request, CancellationToken cancellationToken)
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

        // Remove related skills first to avoid FK constraint issues
        foreach (var vs in vacancy.VacancySkills.ToList())
            _vacancySkillRepo.Remove(vs);

        _vacancyRepo.Remove(vacancy);

        await _uow.SaveChangesAsync(cancellationToken);
    }
}
