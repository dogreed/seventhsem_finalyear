using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Models.Skills;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Admin.Commands.ManageSkill;

public class CreateSkillCommandHandler : IRequestHandler<CreateSkillCommand, SkillResponse>
{
    private readonly IRepository<Skill> _skillRepo;
    private readonly IUnitOfWork _uow;

    public CreateSkillCommandHandler(IRepository<Skill> skillRepo, IUnitOfWork uow)
    {
        _skillRepo = skillRepo;
        _uow = uow;
    }

    public async Task<SkillResponse> Handle(CreateSkillCommand request, CancellationToken cancellationToken)
    {
        var duplicate = (await _skillRepo.GetAllAsync(cancellationToken))
            .FirstOrDefault(s => s.Name.ToLower() == request.Name.ToLower());
        if (duplicate is not null)
            throw new ConflictException($"Skill '{request.Name}' already exists.");

        var skill = new Skill { Name = request.Name };
        await _skillRepo.AddAsync(skill, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new SkillResponse(skill.Id, skill.Name, []);
    }
}



public class CreateSkillAliasCommandHandler : IRequestHandler<CreateSkillAliasCommand, SkillAliasResponse>
{
    private readonly IRepository<Skill> _skillRepo;
    private readonly IRepository<SkillAlias> _aliasRepo;
    private readonly IUnitOfWork _uow;

    public CreateSkillAliasCommandHandler(
        IRepository<Skill> skillRepo,
        IRepository<SkillAlias> aliasRepo,
        IUnitOfWork uow)
    {
        _skillRepo = skillRepo;
        _aliasRepo = aliasRepo;
        _uow = uow;
    }

    public async Task<SkillAliasResponse> Handle(
        CreateSkillAliasCommand request, CancellationToken cancellationToken)
    {
        var skill = await _skillRepo.GetByIdAsync(request.SkillId, cancellationToken)
            ?? throw new NotFoundException(nameof(Skill), request.SkillId);

        var normalised = request.Alias.ToLower().Trim();

        // Guard 3: alias cannot map to more than one SkillId
        var existingAlias = (await _aliasRepo.GetAllAsync(cancellationToken))
            .FirstOrDefault(a => a.Alias == normalised);

        if (existingAlias is not null)
        {
            if (existingAlias.SkillId != request.SkillId)
                throw new ConflictException(
                    $"Alias '{normalised}' is already mapped to a different skill (SkillId: {existingAlias.SkillId}).");

            throw new ConflictException($"Alias '{normalised}' already exists for this skill.");
        }

        var alias = new SkillAlias { SkillId = skill.Id, Alias = normalised };
        await _aliasRepo.AddAsync(alias, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new SkillAliasResponse(alias.Id, skill.Id, skill.Name, alias.Alias);
    }
}



public class DeleteSkillAliasCommandHandler : IRequestHandler<DeleteSkillAliasCommand>
{
    private readonly IRepository<SkillAlias> _aliasRepo;
    private readonly IUnitOfWork _uow;

    public DeleteSkillAliasCommandHandler(IRepository<SkillAlias> aliasRepo, IUnitOfWork uow)
    {
        _aliasRepo = aliasRepo;
        _uow = uow;
    }

    public async Task Handle(DeleteSkillAliasCommand request, CancellationToken cancellationToken)
    {
        var alias = await _aliasRepo.GetByIdAsync(request.AliasId, cancellationToken)
            ?? throw new NotFoundException(nameof(SkillAlias), request.AliasId);

        _aliasRepo.Remove(alias);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}


