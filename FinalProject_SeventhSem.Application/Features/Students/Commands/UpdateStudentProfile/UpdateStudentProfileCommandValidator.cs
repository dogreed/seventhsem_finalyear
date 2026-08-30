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

namespace FinalProject_SeventhSem.Application.Features.Students.Commands.UpdateStudentProfile;

public class UpdateStudentProfileCommandValidator : AbstractValidator<UpdateStudentProfileCommand>
{
    public UpdateStudentProfileCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PhoneNumber).MaximumLength(20).When(x => x.PhoneNumber is not null);
        RuleFor(x => x.Bio).MaximumLength(1000).When(x => x.Bio is not null);
        RuleFor(x => x.GitHubUrl).MaximumLength(300).When(x => x.GitHubUrl is not null);
        RuleFor(x => x.PortfolioUrl).MaximumLength(300).When(x => x.PortfolioUrl is not null);
        RuleFor(x => x.LinkedInUrl).MaximumLength(300).When(x => x.LinkedInUrl is not null);
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

