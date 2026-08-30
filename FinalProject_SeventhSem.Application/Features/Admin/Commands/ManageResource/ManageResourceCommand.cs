using FinalProject_SeventhSem.Application.Exceptions;
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

namespace FinalProject_SeventhSem.Application.Features.Admin.Commands.ManageResource;

// ── Shared response DTO ───────────────────────────────────────────────────────

public record ResourceDto(
    int ResourceId,
    string Title,
    string? Description,
    string Url,
    string? ResourceType,
    IReadOnlyList<string> MappedSkills,
    double AverageRating,
    int RatingCount
);

// ════════════════════════════════════════════════════════════════════════════
// CREATE RESOURCE
// ════════════════════════════════════════════════════════════════════════════

public record CreateResourceCommand(
    string Title,
    string? Description,
    string Url,
    string? ResourceType,
    IReadOnlyList<int> SkillIds
) : IRequest<ResourceDto>;

public class CreateResourceCommandValidator : AbstractValidator<CreateResourceCommand>
{
    public CreateResourceCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Url).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
        RuleFor(x => x.ResourceType).MaximumLength(50).When(x => x.ResourceType is not null);
    }
}

public class CreateResourceCommandHandler : IRequestHandler<CreateResourceCommand, ResourceDto>
{
    private readonly IRepository<Resource> _resourceRepo;
    private readonly IRepository<ResourceSkillMapping> _mappingRepo;
    private readonly IRepository<Skill> _skillRepo;
    private readonly IUnitOfWork _uow;

    public CreateResourceCommandHandler(
        IRepository<Resource> resourceRepo,
        IRepository<ResourceSkillMapping> mappingRepo,
        IRepository<Skill> skillRepo,
        IUnitOfWork uow)
    {
        _resourceRepo = resourceRepo;
        _mappingRepo = mappingRepo;
        _skillRepo = skillRepo;
        _uow = uow;
    }

    public async Task<ResourceDto> Handle(CreateResourceCommand request, CancellationToken ct)
    {
        var resource = new Resource
        {
            Title = request.Title,
            Description = request.Description,
            Url = request.Url,
            ResourceType = request.ResourceType
        };

        await _resourceRepo.AddAsync(resource, ct);
        await _uow.SaveChangesAsync(ct);

        var allSkills = await _skillRepo.GetAllAsync(ct);
        var skillMap = allSkills.ToDictionary(s => s.Id, s => s.Name);

        foreach (var skillId in request.SkillIds.Distinct())
        {
            if (!skillMap.ContainsKey(skillId)) continue;
            await _mappingRepo.AddAsync(
                new ResourceSkillMapping { ResourceId = resource.Id, SkillId = skillId }, ct);
        }
        await _uow.SaveChangesAsync(ct);

        return new ResourceDto(
            ResourceId: resource.Id,
            Title: resource.Title,
            Description: resource.Description,
            Url: resource.Url,
            ResourceType: resource.ResourceType,
            MappedSkills: request.SkillIds
                .Where(skillMap.ContainsKey)
                .Select(id => skillMap[id])
                .ToList(),
            AverageRating: 0,
            RatingCount: 0);
    }
}

// ════════════════════════════════════════════════════════════════════════════
// UPDATE RESOURCE
// ════════════════════════════════════════════════════════════════════════════

public record UpdateResourceCommand(
    int ResourceId,
    string Title,
    string? Description,
    string Url,
    string? ResourceType,
    IReadOnlyList<int> SkillIds
) : IRequest<ResourceDto>;

public class UpdateResourceCommandValidator : AbstractValidator<UpdateResourceCommand>
{
    public UpdateResourceCommandValidator()
    {
        RuleFor(x => x.ResourceId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Url).NotEmpty().MaximumLength(500);
    }
}

public class UpdateResourceCommandHandler : IRequestHandler<UpdateResourceCommand, ResourceDto>
{
    private readonly IRepository<Resource> _resourceRepo;
    private readonly IRepository<ResourceSkillMapping> _mappingRepo;
    private readonly IRepository<Skill> _skillRepo;
    private readonly IUnitOfWork _uow;

    public UpdateResourceCommandHandler(
        IRepository<Resource> resourceRepo,
        IRepository<ResourceSkillMapping> mappingRepo,
        IRepository<Skill> skillRepo,
        IUnitOfWork uow)
    {
        _resourceRepo = resourceRepo;
        _mappingRepo = mappingRepo;
        _skillRepo = skillRepo;
        _uow = uow;
    }

    public async Task<ResourceDto> Handle(UpdateResourceCommand request, CancellationToken ct)
    {
        //var resource = await _resourceRepo.GetByIdAsync(request.ResourceId, ct)
        //    ?? throw new NotFoundException(nameof(Resource), request.ResourceId);

        var resource = await _resourceRepo.GetByIdAsync(
               id: request.ResourceId,
               include: q => q.Include(r => r.SkillMappings)
                              .Include(r => r.Ratings),
               cancellationToken: ct)
               ?? throw new NotFoundException(nameof(Resource), request.ResourceId);

        resource.Title = request.Title;
        resource.Description = request.Description;
        resource.Url = request.Url;
        resource.ResourceType = request.ResourceType;
        resource.UpdatedAt = DateTime.UtcNow;
        _resourceRepo.Update(resource);

        // Replace skill mappings
        foreach (var m in resource.SkillMappings.ToList())
            _mappingRepo.Remove(m);
        await _uow.SaveChangesAsync(ct);

        var allSkills = await _skillRepo.GetAllAsync(ct);
        var skillMap = allSkills.ToDictionary(s => s.Id, s => s.Name);

        foreach (var skillId in request.SkillIds.Distinct().Where(skillMap.ContainsKey))
            await _mappingRepo.AddAsync(
                new ResourceSkillMapping { ResourceId = resource.Id, SkillId = skillId }, ct);

        await _uow.SaveChangesAsync(ct);

        double avgRating = resource.Ratings.Any()
            ? resource.Ratings.Average(r => r.Rating) : 0;

        return new ResourceDto(
            ResourceId: resource.Id,
            Title: resource.Title,
            Description: resource.Description,
            Url: resource.Url,
            ResourceType: resource.ResourceType,
            MappedSkills: request.SkillIds.Where(skillMap.ContainsKey).Select(id => skillMap[id]).ToList(),
            AverageRating: Math.Round(avgRating, 2),
            RatingCount: resource.Ratings.Count);
    }
}

// ════════════════════════════════════════════════════════════════════════════
// DELETE RESOURCE
// ════════════════════════════════════════════════════════════════════════════

public record DeleteResourceCommand(int ResourceId) : IRequest;

public class DeleteResourceCommandHandler : IRequestHandler<DeleteResourceCommand>
{
    private readonly IRepository<Resource> _resourceRepo;
    private readonly IUnitOfWork _uow;

    public DeleteResourceCommandHandler(IRepository<Resource> resourceRepo, IUnitOfWork uow)
    {
        _resourceRepo = resourceRepo;
        _uow = uow;
    }

    public async Task Handle(DeleteResourceCommand request, CancellationToken ct)
    {
        var resource = await _resourceRepo.GetByIdAsync(request.ResourceId, ct)
            ?? throw new NotFoundException(nameof(Resource), request.ResourceId);

        _resourceRepo.Remove(resource);
        await _uow.SaveChangesAsync(ct);
    }
}

// ════════════════════════════════════════════════════════════════════════════
// GET ALL RESOURCES
// ════════════════════════════════════════════════════════════════════════════

public record GetAllResourcesQuery : IRequest<IReadOnlyList<ResourceDto>>;

public class GetAllResourcesQueryHandler : IRequestHandler<GetAllResourcesQuery, IReadOnlyList<ResourceDto>>
{
    private readonly IRepository<Resource> _resourceRepo;

    public GetAllResourcesQueryHandler(IRepository<Resource> resourceRepo)
        => _resourceRepo = resourceRepo;

    public async Task<IReadOnlyList<ResourceDto>> Handle(GetAllResourcesQuery request, CancellationToken ct)
    {
        //var resources = await _resourceRepo.GetAllAsync(ct);
        var resources = await _resourceRepo.GetAllAsync(
           include: q => q.Include(r => r.Ratings)
                          .Include(r => r.SkillMappings)
                              .ThenInclude(m => m.Skill),
           cancellationToken: ct);
        return resources
            .OrderBy(r => r.Title)
            .Select(r =>
            {
                double avg = r.Ratings.Any() ? r.Ratings.Average(x => x.Rating) : 0;
                return new ResourceDto(
                    ResourceId: r.Id,
                    Title: r.Title,
                    Description: r.Description,
                    Url: r.Url,
                    ResourceType: r.ResourceType,
                    MappedSkills: r.SkillMappings.Select(m => m.Skill.Name).ToList(),
                    AverageRating: Math.Round(avg, 2),
                    RatingCount: r.Ratings.Count);
            })
            .ToList();
    }
}

