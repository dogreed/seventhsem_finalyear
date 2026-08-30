using FinalProject_SeventhSem.Application.Common;
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

namespace FinalProject_SeventhSem.Application.Features.Students.Commands.SetStudentSkills;

// ── Command ───────────────────────────────────────────────────────────────────

/// <summary>
/// Replaces the student's confirmed skills with the provided SkillIds.
/// Called after the student reviews resume parsing suggestions.
/// </summary>
public record SetStudentSkillsCommand(
    int UserId,
    IReadOnlyList<int> ConfirmedSkillIds
) : IRequest;

// ── Validator ─────────────────────────────────────────────────────────────────

public class SetStudentSkillsCommandValidator : AbstractValidator<SetStudentSkillsCommand>
{
    public SetStudentSkillsCommandValidator()
    {
        RuleFor(x => x.ConfirmedSkillIds)
            .NotNull()
            .Must(ids => ids.Count <= 30)
            .WithMessage("Cannot confirm more than 30 skills at once.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public class SetStudentSkillsCommandHandler : IRequestHandler<SetStudentSkillsCommand>
{
    private readonly IRepository<Student> _studentRepo;
    private readonly IRepository<StudentSkill> _skillRepo;
    private readonly IUnitOfWork _uow;

    public SetStudentSkillsCommandHandler(
        IRepository<Student> studentRepo,
        IRepository<StudentSkill> skillRepo,
        IUnitOfWork uow)
    {
        _studentRepo = studentRepo;
        _skillRepo = skillRepo;
        _uow = uow;
    }

    public async Task Handle(SetStudentSkillsCommand request, CancellationToken cancellationToken)
    {
        var student = await StudentResolver.ResolveAsync(request.UserId, _studentRepo, cancellationToken);

        // Remove existing confirmed skills
        foreach (var existing in student.StudentSkills.ToList())
            _skillRepo.Remove(existing);

        // Add new confirmed skills (deduplicated)
        foreach (var skillId in request.ConfirmedSkillIds.Distinct())
        {
            await _skillRepo.AddAsync(new StudentSkill
            {
                StudentId = student.Id,
                SkillId = skillId,
                ConfirmedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        await _uow.SaveChangesAsync(cancellationToken);
    }
}
