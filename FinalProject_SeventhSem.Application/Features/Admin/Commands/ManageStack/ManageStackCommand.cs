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

namespace FinalProject_SeventhSem.Application.Features.Admin.Commands.ManageStack;

public record CreateStackCommand(string Name) : IRequest<StackResponse>;

public class CreateStackCommandValidator : AbstractValidator<CreateStackCommand>
{
    public CreateStackCommandValidator()
        => RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
}

public class CreateStackCommandHandler : IRequestHandler<CreateStackCommand, StackResponse>
{
    private readonly IRepository<Stack> _stackRepo;
    private readonly IUnitOfWork _uow;

    public CreateStackCommandHandler(IRepository<Stack> stackRepo, IUnitOfWork uow)
    {
        _stackRepo = stackRepo;
        _uow = uow;
    }

    public async Task<StackResponse> Handle(CreateStackCommand request, CancellationToken ct)
    {
        var all = await _stackRepo.GetAllAsync(ct);
        if (all.Any(s => s.Name.ToLower() == request.Name.ToLower()))
            throw new ConflictException($"Stack '{request.Name}' already exists.");

        var stack = new Stack { Name = request.Name };
        await _stackRepo.AddAsync(stack, ct);
        await _uow.SaveChangesAsync(ct);

        return new StackResponse(stack.Id, stack.Name, ChapterCount: 0, TotalQuestions: 0);
    }
}

// ════════════════════════════════════════════════════════════════════════════
// UPDATE STACK
// ════════════════════════════════════════════════════════════════════════════

public record UpdateStackCommand(int StackId, string Name) : IRequest<StackResponse>;

public class UpdateStackCommandValidator : AbstractValidator<UpdateStackCommand>
{
    public UpdateStackCommandValidator()
    {
        RuleFor(x => x.StackId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class UpdateStackCommandHandler : IRequestHandler<UpdateStackCommand, StackResponse>
{
    private readonly IRepository<Stack> _stackRepo;
    private readonly IUnitOfWork _uow;

    public UpdateStackCommandHandler(IRepository<Stack> stackRepo, IUnitOfWork uow)
    {
        _stackRepo = stackRepo;
        _uow = uow;
    }

    public async Task<StackResponse> Handle(UpdateStackCommand request, CancellationToken ct)
    {
        var stack = await _stackRepo.GetByIdAsync(request.StackId, ct)
            ?? throw new NotFoundException(nameof(Stack), request.StackId);

        stack.Name = request.Name;
        stack.UpdatedAt = DateTime.UtcNow;
        _stackRepo.Update(stack);
        await _uow.SaveChangesAsync(ct);

        return new StackResponse(
            StackId: stack.Id,
            Name: stack.Name,
            ChapterCount: stack.Chapters.Count,
            TotalQuestions: stack.Chapters.Sum(c => c.Questions.Count));
    }
}

// ════════════════════════════════════════════════════════════════════════════
// DELETE STACK
// ════════════════════════════════════════════════════════════════════════════

public record DeleteStackCommand(int StackId) : IRequest;

public class DeleteStackCommandHandler : IRequestHandler<DeleteStackCommand>
{
    private readonly IRepository<Stack> _stackRepo;
    private readonly IUnitOfWork _uow;

    public DeleteStackCommandHandler(IRepository<Stack> stackRepo, IUnitOfWork uow)
    {
        _stackRepo = stackRepo;
        _uow = uow;
    }

    public async Task Handle(DeleteStackCommand request, CancellationToken ct)
    {
        var stack = await _stackRepo.GetByIdAsync(request.StackId, ct)
            ?? throw new NotFoundException(nameof(Stack), request.StackId);

        _stackRepo.Remove(stack);
        await _uow.SaveChangesAsync(ct);
    }
}

// ════════════════════════════════════════════════════════════════════════════
// GET ALL STACKS
// ════════════════════════════════════════════════════════════════════════════

public record GetAllStacksQuery : IRequest<IReadOnlyList<StackResponse>>;

public class GetAllStacksQueryHandler : IRequestHandler<GetAllStacksQuery, IReadOnlyList<StackResponse>>
{
    private readonly IRepository<Stack> _stackRepo;

    public GetAllStacksQueryHandler(IRepository<Stack> stackRepo)
        => _stackRepo = stackRepo;

    public async Task<IReadOnlyList<StackResponse>> Handle(GetAllStacksQuery request, CancellationToken ct)
    {
        var stacks = await _stackRepo.GetAllAsync(
            include: q => q.Include(s => s.Chapters)
                           .ThenInclude(c => c.Questions),
            cancellationToken: ct);
        //var stacks = await _stackRepo.GetAllAsync(ct);
        return stacks
            .OrderBy(s => s.Name)
            .Select(s => new StackResponse(
                StackId: s.Id,
                Name: s.Name,
                ChapterCount: s.Chapters.Count,
                TotalQuestions: s.Chapters.Sum(c => c.Questions.Count)))
            .ToList();
    }
}

