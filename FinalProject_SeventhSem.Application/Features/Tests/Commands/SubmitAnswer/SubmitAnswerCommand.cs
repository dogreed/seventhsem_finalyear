using FinalProject_SeventhSem.Application.Common.Settings;
using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Features.Tests.Commands.SubmitTest;
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

namespace FinalProject_SeventhSem.Application.Features.Tests.Commands.SubmitAnswer;

public record SubmitAnswerCommand(
    int TestId,
    int UserId,
    int QuestionId,
    string SelectedOption   // "A" | "B" | "C" | "D"
) : IRequest;

// ── Validator ─────────────────────────────────────────────────────────────────

public class SubmitAnswerCommandValidator : AbstractValidator<SubmitAnswerCommand>
{
    private static readonly string[] ValidOptions = ["A", "B", "C", "D"];

    public SubmitAnswerCommandValidator()
    {
        RuleFor(x => x.TestId).GreaterThan(0);
        RuleFor(x => x.QuestionId).GreaterThan(0);
        RuleFor(x => x.SelectedOption)
            .NotEmpty()
            .Must(o => ValidOptions.Contains(o.ToUpper()))
            .WithMessage("SelectedOption must be A, B, C, or D.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public class SubmitAnswerCommandHandler : IRequestHandler<SubmitAnswerCommand>
{
    private readonly IRepository<Test> _testRepo;
    private readonly IRepository<Question> _questionRepo;
    private readonly IRepository<TestAnswer> _answerRepo;
    private readonly IRepository<Student> _studentRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMediator _mediator;

    public SubmitAnswerCommandHandler(
        IRepository<Test> testRepo,
        IRepository<Question> questionRepo,
        IRepository<TestAnswer> answerRepo,
         IRepository<Student> studentRepo,
        IUnitOfWork uow,
        IMediator mediator)
    {
        _testRepo = testRepo;
        _questionRepo = questionRepo;
        _answerRepo = answerRepo;
        _studentRepo = studentRepo;
        _uow = uow;
        _mediator = mediator;
    }

    public async Task Handle(SubmitAnswerCommand request, CancellationToken cancellationToken)
    {
        var test = await _testRepo.GetByIdAsync(request.TestId, cancellationToken)
            ?? throw new NotFoundException(nameof(Test), request.TestId);

        var studentAll = await _studentRepo.GetAllAsync(cancellationToken);
        var student = studentAll.FirstOrDefault(s => s.UserId == request.UserId)
            ?? throw new NotFoundException("No student profile found for this user.");

        if (test.StudentId != student.Id)   // ✅ StudentId vs StudentId now
            throw new UnauthorizedException("This test does not belong to you.");

        //if (test.StudentId != request.UserId)
        //    throw new UnauthorizedException("This test does not belong to you.");

        if (test.Status != TestStatus.InProgress)
            throw new BadRequestException("This test is no longer active.");

        // Time limit check — auto-submit if expired
        if (DateTime.UtcNow > test.ExpiresAt)
        {
            await _mediator.Send(
                new SubmitTest.SubmitTestCommand(request.TestId, request.UserId),
                cancellationToken);
            throw new TestExpiredException();
        }

        var question = await _questionRepo.GetByIdAsync(request.QuestionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Question), request.QuestionId);

        // Prevent duplicate answers for the same question in the same test
        var existingAnswer = (await _answerRepo.GetAllAsync(cancellationToken))
            .FirstOrDefault(a => a.TestId == request.TestId && a.QuestionId == request.QuestionId);

        if (existingAnswer is not null)
            throw new ConflictException("You have already answered this question.");

        bool isCorrect = string.Equals(
            request.SelectedOption.ToUpper(),
            question.CorrectOption.ToUpper(),
            StringComparison.Ordinal);

        await _answerRepo.AddAsync(new TestAnswer
        {
            TestId = request.TestId,
            QuestionId = request.QuestionId,
            SelectedOption = request.SelectedOption.ToUpper(),
            IsCorrect = isCorrect
        }, cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);
    }
}
