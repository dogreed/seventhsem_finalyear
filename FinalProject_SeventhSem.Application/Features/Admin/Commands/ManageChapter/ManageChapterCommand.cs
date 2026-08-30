using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Models.Stacks;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Admin.Commands.ManageChapter;

// ════════════════════════════════════════════════════════════════════════════
// CREATE CHAPTER
// ════════════════════════════════════════════════════════════════════════════

public record CreateChapterCommand(int StackId, string Name) : IRequest<ChapterResponse>;

public class CreateChapterCommandValidator : AbstractValidator<CreateChapterCommand>
{
    public CreateChapterCommandValidator()
    {
        RuleFor(x => x.StackId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}

public class CreateChapterCommandHandler : IRequestHandler<CreateChapterCommand, ChapterResponse>
{
    private readonly IRepository<Chapter> _chapterRepo;
    private readonly IRepository<Stack> _stackRepo;
    private readonly IUnitOfWork _uow;

    public CreateChapterCommandHandler(
        IRepository<Chapter> chapterRepo,
        IRepository<Stack> stackRepo,
        IUnitOfWork uow)
    {
        _chapterRepo = chapterRepo;
        _stackRepo = stackRepo;
        _uow = uow;
    }

    public async Task<ChapterResponse> Handle(CreateChapterCommand request, CancellationToken ct)
    {
        var stack = await _stackRepo.GetByIdAsync(request.StackId, ct)
            ?? throw new NotFoundException(nameof(Stack), request.StackId);

        var chapter = new Chapter { StackId = stack.Id, Name = request.Name };
        await _chapterRepo.AddAsync(chapter, ct);
        await _uow.SaveChangesAsync(ct);

        return new ChapterResponse(
            ChapterId: chapter.Id,
            StackId: stack.Id,
            StackName: stack.Name,
            Name: chapter.Name,
            QuestionCount: 0);
    }
}

// ════════════════════════════════════════════════════════════════════════════
// UPDATE CHAPTER
// ════════════════════════════════════════════════════════════════════════════

public record UpdateChapterCommand(int ChapterId, string Name) : IRequest<ChapterResponse>;

public class UpdateChapterCommandValidator : AbstractValidator<UpdateChapterCommand>
{
    public UpdateChapterCommandValidator()
    {
        RuleFor(x => x.ChapterId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}

public class UpdateChapterCommandHandler : IRequestHandler<UpdateChapterCommand, ChapterResponse>
{
    private readonly IRepository<Chapter> _chapterRepo;
    private readonly IUnitOfWork _uow;

    public UpdateChapterCommandHandler(IRepository<Chapter> chapterRepo, IUnitOfWork uow)
    {
        _chapterRepo = chapterRepo;
        _uow = uow;
    }

    public async Task<ChapterResponse> Handle(UpdateChapterCommand request, CancellationToken ct)
    {
        //var chapter = await _chapterRepo.GetByIdAsync(request.ChapterId, ct)
        //    ?? throw new NotFoundException(nameof(Chapter), request.ChapterId);

        var chapter = await _chapterRepo.GetByIdAsync(
      id: request.ChapterId,
      include: q => q.Include(c => c.Stack).Include(c => c.Questions),
      cancellationToken: ct)
      ?? throw new NotFoundException(nameof(Chapter), request.ChapterId);

        chapter.Name = request.Name;
        chapter.UpdatedAt = DateTime.UtcNow;
        _chapterRepo.Update(chapter);
        await _uow.SaveChangesAsync(ct);

        return new ChapterResponse(
            ChapterId: chapter.Id,
            StackId: chapter.StackId,
            StackName: chapter.Stack.Name,
            Name: chapter.Name,
            QuestionCount: chapter.Questions.Count);
    }
}

// ════════════════════════════════════════════════════════════════════════════
// DELETE CHAPTER
// ════════════════════════════════════════════════════════════════════════════

public record DeleteChapterCommand(int ChapterId) : IRequest;

public class DeleteChapterCommandHandler : IRequestHandler<DeleteChapterCommand>
{
    private readonly IRepository<Chapter> _chapterRepo;
    private readonly IUnitOfWork _uow;

    public DeleteChapterCommandHandler(IRepository<Chapter> chapterRepo, IUnitOfWork uow)
    {
        _chapterRepo = chapterRepo;
        _uow = uow;
    }

    public async Task Handle(DeleteChapterCommand request, CancellationToken ct)
    {
        var chapter = await _chapterRepo.GetByIdAsync(request.ChapterId, ct)
            ?? throw new NotFoundException(nameof(Chapter), request.ChapterId);

        _chapterRepo.Remove(chapter);
        await _uow.SaveChangesAsync(ct);
    }
}

// ════════════════════════════════════════════════════════════════════════════
// GET CHAPTERS BY STACK
// ════════════════════════════════════════════════════════════════════════════

public record GetChaptersByStackQuery(int StackId) : IRequest<IReadOnlyList<ChapterResponse>>;

public class GetChaptersByStackQueryHandler
    : IRequestHandler<GetChaptersByStackQuery, IReadOnlyList<ChapterResponse>>
{
    private readonly IRepository<Chapter> _chapterRepo;

    public GetChaptersByStackQueryHandler(IRepository<Chapter> chapterRepo)
        => _chapterRepo = chapterRepo;

    public async Task<IReadOnlyList<ChapterResponse>> Handle(
        GetChaptersByStackQuery request, CancellationToken ct)
    {
        //var all = await _chapterRepo.GetAllAsync(ct);

        var all = await _chapterRepo.GetAllAsync(
       include: q => q.Include(o => o.Stack).Include(o => o.Questions),
       cancellationToken: ct);
        return all
            .Where(c => c.StackId == request.StackId)
            .OrderBy(c => c.Name)
            .Select(c => new ChapterResponse(
                ChapterId: c.Id,
                StackId: c.StackId,
                StackName: c.Stack.Name,
                Name: c.Name,
                QuestionCount: c.Questions.Count))
            .ToList();
    }
}

