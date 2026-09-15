using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Interfaces;
using FinalProject_SeventhSem.Application.Model;
using FinalProject_SeventhSem.Application.Models.Tests;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace FinalProject_SeventhSem.Application.Features.Resources.Commands.GetRecommendResources;

public class GetRecommendedResourcesQueryHandler
	: IRequestHandler<GetRecommendedResourcesQuery,
		IReadOnlyList<ResourceRecommendationDto>>
{
	private readonly IRepository<Student> _studentRepo;
	private readonly IRepository<TestResult> _testResultRepo;
	private readonly IRepository<Resource> _resourceRepo;
	private readonly IRepository<Chapter> _chapterRepo;
	private readonly IRecommendationService _recommendationService;

	public GetRecommendedResourcesQueryHandler(
		IRepository<Student> studentRepo,
		IRepository<TestResult> testResultRepo,
		IRepository<Resource> resourceRepo,
		IRecommendationService recommendationService,
		IRepository<Chapter> chapterRepo)
	{
		_studentRepo = studentRepo;
		_testResultRepo = testResultRepo;
		_resourceRepo = resourceRepo;
		_chapterRepo = chapterRepo;
		_recommendationService = recommendationService;
	}

	public async Task<IReadOnlyList<ResourceRecommendationDto>> Handle(
		GetRecommendedResourcesQuery request,
		CancellationToken cancellationToken)
	{
		var student = await _studentRepo.GetByIdAsync(
			request.StudentId,
			include: q => q
				.Include(s => s.StudentSkills)
				.ThenInclude(ss => ss.Skill),
			cancellationToken)
			?? throw new NotFoundException(
				nameof(Student),
				request.StudentId);

		// Get latest test result
		var latestResult = (await _testResultRepo.GetAllAsync(cancellationToken))
			.FirstOrDefault(tr =>
				tr.StudentId == request.StudentId &&
				tr.IsLatest);

		var recommendations = new List<ResourceRecommendationDto>();

		if (latestResult is null)
			return recommendations;

		// Get weak chapter IDs
		var weakIds =
			System.Text.Json.JsonSerializer
				.Deserialize<List<int>>(
					latestResult.WeakChapterIdsJson)
			?? [];

		if (weakIds.Count == 0)
			return recommendations;

		// Get chapters
		var chapters = await _chapterRepo.GetAllAsync(
			cancellationToken);

		var chapterMap = chapters.ToDictionary(c => c.Id);

		// Convert weak chapter IDs to names
		var weakChapterNames = weakIds
			.Select(id => chapterMap.GetValueOrDefault(id)?.Name)
			.Where(name => !string.IsNullOrWhiteSpace(name))
			.Select(Normalize)
			.ToList();

		// Get resources and their skills
		var allResources = await _resourceRepo.GetAllAsync(
			include: q => q
				.Include(r => r.SkillMappings)
				.ThenInclude(sm => sm.Skill),
			cancellationToken);

		// Calculate AI scores
		var scoredResources =
			new List<(Resource Resource, double Score)>();

		foreach (var resource in allResources)
		{
			var features = CalculateFeatures(
				student,
				resource);

			var score =
				await _recommendationService.PredictScoreAsync(
					features,
					cancellationToken);

			scoredResources.Add(
				(resource, score));
		}

		var added = new HashSet<int>();

		// =========================================================
		// 1. RECOMMEND RESOURCES FOR WEAK CHAPTERS
		// =========================================================

		foreach (var item in scoredResources
			.OrderByDescending(x => x.Score))
		{
			var resource = item.Resource;

			bool linkedToWeakChapter =
				resource.SkillMappings.Any(mapping =>
					weakChapterNames.Any(weakChapter =>
					{
						var skillName =
							Normalize(mapping.Skill.Name);

						return weakChapter.Contains(skillName)
							   || skillName.Contains(weakChapter);
					}));

			if (!linkedToWeakChapter)
				continue;

			var matchedChapter = weakIds
				.Select(id => chapterMap.GetValueOrDefault(id))
				.FirstOrDefault(chapter =>
					chapter != null &&
					resource.SkillMappings.Any(mapping =>
						Normalize(chapter.Name)
							.Contains(
								Normalize(mapping.Skill.Name))
						||
						Normalize(mapping.Skill.Name)
							.Contains(
								Normalize(chapter.Name))));

			recommendations.Add(
				new ResourceRecommendationDto(
					ResourceId: resource.Id,
					Title: resource.Title,
					Url: resource.Url,
					ResourceType: resource.ResourceType,
					RecommendedBecause:
						$"Weak chapter: {matchedChapter?.Name ?? "Unknown"}"));

			added.Add(resource.Id);
		}

		// =========================================================
		// 2. RECOMMEND RESOURCES FOR MISSING SKILLS
		// =========================================================

		var studentSkillIds = student.StudentSkills
			.Select(ss => ss.SkillId)
			.ToHashSet();

		foreach (var item in scoredResources
			.Where(x => !added.Contains(x.Resource.Id))
			.OrderByDescending(x => x.Score))
		{
			var resource = item.Resource;

			var missingSkill = resource.SkillMappings
				.Select(mapping => mapping.Skill)
				.FirstOrDefault(skill =>
					!studentSkillIds.Contains(skill.Id));

			if (missingSkill is null)
				continue;

			recommendations.Add(
				new ResourceRecommendationDto(
					ResourceId: resource.Id,
					Title: resource.Title,
					Url: resource.Url,
					ResourceType: resource.ResourceType,
					RecommendedBecause:
						$"Missing skill: {missingSkill.Name}"));

			added.Add(resource.Id);
		}

		return recommendations;
	}

	private static RecommendationFeatures CalculateFeatures(
		Student student,
		Resource resource)
	{
		var studentSkillIds = student.StudentSkills
			.Select(ss => ss.SkillId)
			.ToHashSet();

		var resourceSkillIds = resource.SkillMappings
			.Select(sm => sm.SkillId)
			.Distinct()
			.ToList();

		var matchingSkills = resourceSkillIds
			.Count(skillId =>
				studentSkillIds.Contains(skillId));

		var resourceSkillCount =
			resourceSkillIds.Count;

		var matchPercentage =
			resourceSkillCount == 0
				? 0
				: (double)matchingSkills /
				  resourceSkillCount;

		return new RecommendationFeatures(
			ResourceId: resource.Id,
			MatchingSkills: matchingSkills,
			ResourceSkillCount: resourceSkillCount,
			MatchPercentage: matchPercentage);
	}

	private static string Normalize(string value)
	{
		// Remove "Chapter 1-", "Chapter 2:", etc.
		value = Regex.Replace(
			value,
			@"^Chapter\s+\d+\s*[-:]\s*",
			"",
			RegexOptions.IgnoreCase);

		return value
			.Trim()
			.ToLowerInvariant();
	}
}