using FinalProject_SeventhSem.Application.Common.Settings;
using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Admin.Commands.ManageSkill;

public class CreateSkillCommandValidator : AbstractValidator<CreateSkillCommand>
{
    public CreateSkillCommandValidator()
        => RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
}

public class CreateSkillAliasCommandValidator : AbstractValidator<CreateSkillAliasCommand>
{
    private readonly ResumeParsingSettings _parsingSettings;

    public CreateSkillAliasCommandValidator(IOptions<ResumeParsingSettings> parsingSettings)
    {
        _parsingSettings = parsingSettings.Value;

        RuleFor(x => x.SkillId).GreaterThan(0);
        RuleFor(x => x.Alias)
            .NotEmpty()
            .MinimumLength(2).WithMessage("Alias minimum length is 2.")
            .MaximumLength(100)
            .Must(alias => !_parsingSettings.StopWords.Contains(alias.ToLower()))
            .WithMessage("Alias cannot be a stopword.");
    }
}


public class DeleteSkillAliasCommandValidator : AbstractValidator<DeleteSkillAliasCommand>
{
    public DeleteSkillAliasCommandValidator()
        => RuleFor(x => x.AliasId).GreaterThan(0);
}

