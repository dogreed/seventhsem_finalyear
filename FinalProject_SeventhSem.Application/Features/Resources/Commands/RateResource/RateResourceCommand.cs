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

namespace FinalProject_SeventhSem.Application.Features.Resources.Commands.RateResource;

public record RateResourceCommand(
    int ResourceId,
    int StudentId,
    int Rating,
    string? Comment
) : IRequest;

// ── Validator ─────────────────────────────────────────────────────────────────

public class RateResourceCommandValidator : AbstractValidator<RateResourceCommand>
{
    public RateResourceCommandValidator()
    {
        RuleFor(x => x.ResourceId).GreaterThan(0);
        RuleFor(x => x.StudentId).GreaterThan(0);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5.");
        RuleFor(x => x.Comment).MaximumLength(500).When(x => x.Comment is not null);
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

