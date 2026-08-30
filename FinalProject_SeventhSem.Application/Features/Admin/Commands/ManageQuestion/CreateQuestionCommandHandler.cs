using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Models.Stacks;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Admin.Commands.ManageQuestion;

public class CreateQuestionCommandHandler : IRequestHandler<CreateQuestionCommand, QuestionResponse>
{
    private readonly IRepository<Question> _questionRepo;
    private readonly IRepository<Chapter> _chapterRepo;
    private readonly IUnitOfWork _uow;

    public CreateQuestionCommandHandler(
        IRepository<Question> questionRepo,
        IRepository<Chapter> chapterRepo,
        IUnitOfWork uow)
    {
        _questionRepo = questionRepo;
        _chapterRepo = chapterRepo;
        _uow = uow;
    }

    public async Task<QuestionResponse> Handle(
         CreateQuestionCommand request, CancellationToken cancellationToken)
    {
        // Fetch Chapter and eagerly load Stack to verify relationship mapping
        var chapter = await _chapterRepo.GetAsync(
            predicate: c => c.Id == request.ChapterId,
            include: q => q.Include(c => c.Stack),
            cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(Chapter), request.ChapterId);

        // Validation rule: Confirm that the selected chapter belongs to the requested tech stack
        if (chapter.StackId != request.StackId)
        {
            throw new BadRequestException($"Chapter '{chapter.Name}' does not belong to the requested Stack ID {request.StackId}.");
        }

        var question = new Question
        {
            ChapterId = chapter.Id,
            Text = request.Text,
            OptionA = request.OptionA,
            OptionB = request.OptionB,
            OptionC = request.OptionC,
            OptionD = request.OptionD,
            CorrectOption = request.CorrectOption.ToUpper()
        };

        await _questionRepo.AddAsync(question, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new QuestionResponse(
            QuestionId: question.Id,
            ChapterId: chapter.Id,
            ChapterName: chapter.Name,
            StackName: chapter.Stack.Name,
            Text: question.Text,
            OptionA: question.OptionA,
            OptionB: question.OptionB,
            OptionC: question.OptionC,
            OptionD: question.OptionD,
            CorrectOption: question.CorrectOption);
    }
}

/// <summary>
/// Update the question
/// </summary>
/// <param name="QuestionId"></param>



public record PatchQuestionCommand(
    int QuestionId,
    string? Text,
    string? OptionA,
    string? OptionB,
    string? OptionC,
    string? OptionD,
    string? CorrectOption
) : IRequest<QuestionResponse>;

public class PatchQuestionCommandHandler : IRequestHandler<PatchQuestionCommand, QuestionResponse>
{
    private readonly IRepository<Question> _questionRepo;
    private readonly IUnitOfWork _uow;

    public PatchQuestionCommandHandler(IRepository<Question> questionRepo, IUnitOfWork uow)
    {
        _questionRepo = questionRepo;
        _uow = uow;
    }

    public async Task<QuestionResponse> Handle(
        PatchQuestionCommand request, CancellationToken cancellationToken)
    {
        // Fetch the question and its parental contextual tags for response construction
        var question = await _questionRepo.GetAsync(
            predicate: q => q.Id == request.QuestionId,
            include: query => query.Include(q => q.Chapter).ThenInclude(c => c.Stack),
            cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(Question), request.QuestionId);

        // Apply mutations conditionally only if values are present in the PATCH body
        if (request.Text != null) question.Text = request.Text;
        if (request.OptionA != null) question.OptionA = request.OptionA;
        if (request.OptionB != null) question.OptionB = request.OptionB;
        if (request.OptionC != null) question.OptionC = request.OptionC;
        if (request.OptionD != null) question.OptionD = request.OptionD;
        if (!string.IsNullOrWhiteSpace(request.CorrectOption)) question.CorrectOption = request.CorrectOption.ToUpper();

        _questionRepo.Update(question);
        await _uow.SaveChangesAsync(cancellationToken);

        return new QuestionResponse(
            QuestionId: question.Id,
            ChapterId: question.ChapterId,
            ChapterName: question.Chapter.Name,
            StackName: question.Chapter.Stack.Name,
            Text: question.Text,
            OptionA: question.OptionA,
            OptionB: question.OptionB,
            OptionC: question.OptionC,
            OptionD: question.OptionD,
            CorrectOption: question.CorrectOption);
    }
}


/// <summary>
/// delete the question
/// </summary>
/// <param name="QuestionId"></param>

public record DeleteQuestionCommand(int QuestionId) : IRequest;

public class DeleteQuestionCommandHandler : IRequestHandler<DeleteQuestionCommand>
{
    private readonly IRepository<Question> _questionRepo;
    private readonly IUnitOfWork _uow;

    public DeleteQuestionCommandHandler(IRepository<Question> questionRepo, IUnitOfWork uow)
    {
        _questionRepo = questionRepo;
        _uow = uow;
    }

    public async Task Handle(DeleteQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _questionRepo.GetByIdAsync(request.QuestionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Question), request.QuestionId);

        _questionRepo.Remove(question);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}

