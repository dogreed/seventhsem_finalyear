using FinalProject_SeventhSem.Application.Models.Applications;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Applications.Commands.ApplyToVacancy;

public record ApplyToVacancyCommand(int StudentId, int VacancyId) : IRequest<ApplicationResponse>;

// ── Validator ─────────────────────────────────────────────────────────────────

public class ApplyToVacancyCommandValidator : AbstractValidator<ApplyToVacancyCommand>
{
    public ApplyToVacancyCommandValidator()
    {
        RuleFor(x => x.StudentId).GreaterThan(0);
        RuleFor(x => x.VacancyId).GreaterThan(0);
    }
}

