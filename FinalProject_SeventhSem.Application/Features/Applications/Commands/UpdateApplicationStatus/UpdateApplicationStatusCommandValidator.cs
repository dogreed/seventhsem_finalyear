using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Domain.Interfaces;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Applications.Commands.UpdateApplicationStatus;

public class UpdateApplicationStatusCommandValidator
    : AbstractValidator<UpdateApplicationStatusCommand>
{
    public UpdateApplicationStatusCommandValidator()
    {
        RuleFor(x => x.ApplicationId).GreaterThan(0);
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.NewStatus).IsInEnum();
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

