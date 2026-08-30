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

// GET QUESTIONS BY CHAPTER
public record GetQuestionsByChapterQuery(int StackId, int ChapterId) : IRequest<IReadOnlyList<QuestionResponse>>;

public class GetQuestionsByChapterQueryHandler : IRequestHandler<GetQuestionsByChapterQuery, IReadOnlyList<QuestionResponse>>
{
    private readonly IRepository<Chapter> _chapterRepo;

    public GetQuestionsByChapterQueryHandler(IRepository<Chapter> chapterRepo)
        => _chapterRepo = chapterRepo;

    public async Task<IReadOnlyList<QuestionResponse>> Handle(GetQuestionsByChapterQuery request, CancellationToken ct)
    {
        var chapter = await _chapterRepo.GetByIdAsync(
            id: request.ChapterId,
            include: q => q.Include(c => c.Stack).Include(c => c.Questions),
            cancellationToken: ct)
            ?? throw new NotFoundException(nameof(Chapter), request.ChapterId);

        if (chapter.StackId != request.StackId)
            throw new NotFoundException(nameof(Chapter), request.ChapterId);

        return chapter.Questions
            .Select(q => new QuestionResponse(
                QuestionId: q.Id,
                ChapterId: chapter.Id,
                ChapterName: chapter.Name,
                StackName: chapter.Stack.Name,
                Text: q.Text,
                OptionA: q.OptionA,
                OptionB: q.OptionB,
                OptionC: q.OptionC,
                OptionD: q.OptionD,
                CorrectOption: q.CorrectOption))
            .ToList();
    }
}

// GET ALL QUESTIONS BY STACK
public record GetQuestionsByStackQuery(int StackId) : IRequest<IReadOnlyList<QuestionResponse>>;

public class GetQuestionsByStackQueryHandler : IRequestHandler<GetQuestionsByStackQuery, IReadOnlyList<QuestionResponse>>
{
    private readonly IRepository<Stack> _stackRepo;

    public GetQuestionsByStackQueryHandler(IRepository<Stack> stackRepo)
        => _stackRepo = stackRepo;

    public async Task<IReadOnlyList<QuestionResponse>> Handle(GetQuestionsByStackQuery request, CancellationToken ct)
    {
        var stack = await _stackRepo.GetByIdAsync(
            id: request.StackId,
            include: q => q.Include(s => s.Chapters)
                           .ThenInclude(c => c.Questions),
            cancellationToken: ct)
            ?? throw new NotFoundException(nameof(Stack), request.StackId);

        return stack.Chapters
            .SelectMany(c => c.Questions.Select(q => new QuestionResponse(
                QuestionId: q.Id,
                ChapterId: c.Id,
                ChapterName: c.Name,
                StackName: stack.Name,
                Text: q.Text,
                OptionA: q.OptionA,
                OptionB: q.OptionB,
                OptionC: q.OptionC,
                OptionD: q.OptionD,
                CorrectOption: q.CorrectOption)))
            .ToList();
    }
}
