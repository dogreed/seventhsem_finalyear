using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Vacancies.Commands.PublishVacancy;

public class PublishVacancyCommandHandler : IRequestHandler<PublishVacancyCommand>
{
    private readonly IRepository<Vacancy> _vacancyRepo;
    private readonly IUnitOfWork _uow;

    public PublishVacancyCommandHandler(IRepository<Vacancy> vacancyRepo, IUnitOfWork uow)
    {
        _vacancyRepo = vacancyRepo;
        _uow = uow;
    }

    public async Task Handle(PublishVacancyCommand request, CancellationToken cancellationToken)
    {
        var vacancy = await _vacancyRepo.GetByIdAsync(request.VacancyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Vacancy), request.VacancyId);

        if (vacancy.OrganizationId != request.OrganizationId)
            throw new UnauthorizedException("You do not own this vacancy.");

        if (vacancy.IsPublished)
            throw new BadRequestException("Vacancy is already published.");

        // Backend guard: cannot publish without at least 1 required skill
        bool hasRequired = vacancy.VacancySkills.Any(vs => vs.IsRequired);
        if (!hasRequired)
            throw new BadRequestException("Cannot publish a vacancy with no required skills.");

        vacancy.IsPublished = true;
        vacancy.PublishedAt = DateTime.UtcNow;
        vacancy.UpdatedAt = DateTime.UtcNow;

        _vacancyRepo.Update(vacancy);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}


