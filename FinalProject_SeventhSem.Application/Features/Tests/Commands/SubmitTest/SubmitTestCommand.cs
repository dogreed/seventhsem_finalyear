using FinalProject_SeventhSem.Application.Common.Settings;
using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Models.Tests;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Enums;
using FinalProject_SeventhSem.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Tests.Commands.SubmitTest;

public record SubmitTestCommand(int TestId, int UserId) : IRequest<TestResultResponse>;


public class SubmitTestCommandValidator : AbstractValidator<SubmitTestCommand>
{
    public SubmitTestCommandValidator()
    {
        RuleFor(x => x.TestId).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}


