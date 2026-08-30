using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Vacancies.Commands.PublishVacancy;

public record PublishVacancyCommand(int VacancyId, int OrganizationId) : IRequest;

// ── Validator ─────────────────────────────────────────────────────────────────

public class PublishVacancyCommandValidator : AbstractValidator<PublishVacancyCommand>
{
    public PublishVacancyCommandValidator()
    {
        RuleFor(x => x.VacancyId).GreaterThan(0);
        RuleFor(x => x.OrganizationId).GreaterThan(0);
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

