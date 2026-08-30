using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Models.Tests;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Resources.Commands.GetRecommendResources;

public class GetRecommendedResourcesQueryHandler
    : IRequestHandler<GetRecommendedResourcesQuery, IReadOnlyList<ResourceRecommendationDto>>
{
    private readonly IRepository<Student> _studentRepo;
    private readonly IRepository<TestResult> _testResultRepo;
    private readonly IRepository<Resource> _resourceRepo;
    private readonly IRepository<Chapter> _chapterRepo;

    public GetRecommendedResourcesQueryHandler(
        IRepository<Student> studentRepo,
        IRepository<TestResult> testResultRepo,
        IRepository<Resource> resourceRepo,
        IRepository<Chapter> chapterRepo)
    {
        _studentRepo = studentRepo;
        _testResultRepo = testResultRepo;
        _resourceRepo = resourceRepo;
        _chapterRepo = chapterRepo;
    }

    public async Task<IReadOnlyList<ResourceRecommendationDto>> Handle(
        GetRecommendedResourcesQuery request, CancellationToken cancellationToken)
    {
        var student = await _studentRepo.GetByIdAsync(request.StudentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Student), request.StudentId);

        // Get latest test result
        var latestResult = (await _testResultRepo.GetAllAsync(cancellationToken))
            .FirstOrDefault(tr => tr.StudentId == request.StudentId && tr.IsLatest);

        var recommendations = new List<ResourceRecommendationDto>();
        var added = new HashSet<int>();

        if (latestResult is not null)
        {
            var weakIds = System.Text.Json.JsonSerializer
                .Deserialize<List<int>>(latestResult.WeakChapterIdsJson) ?? [];

            var allResources = await _resourceRepo.GetAllAsync(cancellationToken);
            var chapters = await _chapterRepo.GetAllAsync(cancellationToken);
            var chapterMap = chapters.ToDictionary(c => c.Id);

            // Rule-based lookup: weak chapter → stack → resources tagged with skills in that stack
            foreach (var resource in allResources)
            {
                foreach (var mapping in resource.SkillMappings)
                {
                    if (added.Contains(resource.Id)) break;

                    // Find chapters belonging to the same stack as the mapped skill's stack
                    // (Resources link to Skills; Skills link to Stacks via Chapters)
                    bool linkedToWeak = weakIds
                        .Select(id => chapterMap.GetValueOrDefault(id))
                        .Where(c => c is not null)
                        .Any(c => resource.SkillMappings.Any(sm => sm.SkillId == mapping.SkillId));

                    if (linkedToWeak)
                    {
                        var chapter = weakIds
                            .Select(id => chapterMap.GetValueOrDefault(id))
                            .FirstOrDefault(c => c is not null);

                        recommendations.Add(new ResourceRecommendationDto(
                            ResourceId: resource.Id,
                            Title: resource.Title,
                            Url: resource.Url,
                            ResourceType: resource.ResourceType,
                            RecommendedBecause: $"Weak chapter: {chapter?.Name ?? "Unknown"}"));

                        added.Add(resource.Id);
                    }
                }
            }

            // Also recommend for missing skills from applications (Algorithm 11 — MissingSkills path)
            var studentSkillIds = student.StudentSkills.Select(ss => ss.SkillId).ToHashSet();
            foreach (var resource in allResources.Where(r => !added.Contains(r.Id)))
            {
                foreach (var mapping in resource.SkillMappings)
                {
                    if (!studentSkillIds.Contains(mapping.SkillId))
                    {
                        recommendations.Add(new ResourceRecommendationDto(
                            ResourceId: resource.Id,
                            Title: resource.Title,
                            Url: resource.Url,
                            ResourceType: resource.ResourceType,
                            RecommendedBecause: $"Missing skill: {mapping.Skill.Name}"));
                        added.Add(resource.Id);
                        break;
                    }
                }
            }
        }

        return recommendations;
    }
}
