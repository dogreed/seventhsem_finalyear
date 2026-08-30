using FinalProject_SeventhSem.Application.Interfaces;
using FinalProject_SeventhSem.Application.Models.Vacancies;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using Microsoft.Extensions.Options;
using FinalProject_SeventhSem.Application.Common.Settings;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure.Engines;


/// <summary>
/// Implements IMatchingService.
///
/// Algorithm 3 — Education Check (Rule-Based Hard Filter):
///   Compares student EducationLevel enum ordinal to vacancy requirement.
///
/// Algorithm 4 — Requirement Fit (Set Intersection Ratio):
///   matched = |StudentSkills ∩ RequiredSkills|
///   RequirementFit = (matched / total) * 100
///
/// Algorithm 5 — Optional Fit (Set Intersection Ratio):
///   Same formula applied to optional skills.
///
/// Algorithm 6 — Skill Gap (Set Difference):
///   MissingSkills = (RequiredSkills ∪ OptionalSkills) − StudentSkills
/// </summary>
public class MatchingEngine : IMatchingService
{
    private readonly ThresholdSettings _thresholds;
    private readonly IRepository<Skill> _skillRepo;

    public MatchingEngine(IOptions<ThresholdSettings> thresholds, IRepository<Skill> skillRepo)
    {
        _thresholds = thresholds.Value;
        _skillRepo = skillRepo;
    }


    public (bool IsEligible, double EducationBonus) CheckEducation(Student student, Vacancy vacancy)
    {
        if (vacancy.RequiredEducationLevel is null)
            return (true, 0);

        bool meetsLevel = student.EducationLevel.HasValue &&
                          (int)student.EducationLevel.Value >= (int)vacancy.RequiredEducationLevel.Value;

        if (!meetsLevel)
            return (false, 0);

        // Optional field-of-study match → +EducationOptionalBonus
        //double bonus = _thresholds.EducationLevelBonus;
        //if (!string.IsNullOrWhiteSpace(vacancy.RequiredFieldOfStudy) &&
        //    !string.IsNullOrWhiteSpace(student.FieldOfStudy) &&
        //    student.FieldOfStudy.Equals(vacancy.RequiredFieldOfStudy, StringComparison.OrdinalIgnoreCase))
        //{
        //    bonus += _thresholds.EducationOptionalBonus;
        //}

        return (true, _thresholds.EducationLevelBonus);
    }


    public double ComputeRequirementFit(
        IEnumerable<int> studentSkillIds, IEnumerable<int> requiredSkillIds)
    {
        var required = requiredSkillIds.ToHashSet();
        if (required.Count == 0) return 100; 

        var matched = studentSkillIds.Count(id => required.Contains(id));
        return Math.Round((double)matched / required.Count * 100, 2);
    }


    public double ComputeOptionalFit(
        IEnumerable<int> studentSkillIds, IEnumerable<int> optionalSkillIds)
    {
        var optional = optionalSkillIds.ToHashSet();
        if (optional.Count == 0) return 0;

        var matched = studentSkillIds.Count(id => optional.Contains(id));
        return Math.Round((double)matched / optional.Count * 100, 2);
    }


    public IReadOnlyList<int> ComputeSkillGap(
        IEnumerable<int> studentSkillIds,
        IEnumerable<int> requiredSkillIds,
        IEnumerable<int> optionalSkillIds)
    {
        var student = studentSkillIds.ToHashSet();
        var allNeeded = requiredSkillIds.Union(optionalSkillIds).ToHashSet();

        return allNeeded
            .Where(id => !student.Contains(id))
            .ToList();
    }


    public VacancyMatchResult Match(Student student, Vacancy vacancy)
    {
        var studentSkillIds = student.StudentSkills.Select(ss => ss.SkillId).ToList();
        var requiredSkillIds = vacancy.VacancySkills.Where(vs => vs.IsRequired).Select(vs => vs.SkillId).ToList();
        var optionalSkillIds = vacancy.VacancySkills.Where(vs => !vs.IsRequired).Select(vs => vs.SkillId).ToList();

        // Algorithm 3
        var (educationEligible, educationBonus) = CheckEducation(student, vacancy);

        // Algorithm 4
        var requirementFit = ComputeRequirementFit(studentSkillIds, requiredSkillIds);

        // Algorithm 5
        var optionalFit = ComputeOptionalFit(studentSkillIds, optionalSkillIds);

        // Algorithm 6
        var missingIds = ComputeSkillGap(studentSkillIds, requiredSkillIds, optionalSkillIds);

        bool isEligible = educationEligible &&
                          requirementFit >= _thresholds.EligibilityMinPercent;

        string? message = null;
        if (!educationEligible)
            message = $"Required education level not met (requires {vacancy.RequiredEducationLevel}).";
        else if (requirementFit < _thresholds.EligibilityMinPercent)
            message = $"Requirement fit {requirementFit:F1}% is below the minimum {_thresholds.EligibilityMinPercent}%.";

        var skillNameMap = vacancy.VacancySkills
            .Select(vs => vs.Skill)
            .DistinctBy(s => s.Id)
            .ToDictionary(s => s.Id, s => s.Name);

        var missingNames = missingIds
            .Select(id => skillNameMap.GetValueOrDefault(id, "Unknown"))
            .ToList();

        return new VacancyMatchResult(
            VacancyId: vacancy.Id,
            Title: vacancy.Title,
            OrganizationName: vacancy.Organization.Name,
            IsEligible: isEligible,
            EligibilityMessage: message,
            RequirementFit: requirementFit,
            OptionalFit: optionalFit,
            EducationBonus: educationBonus,
            MissingSkills: missingNames,
            MissingSkillIds: missingIds);
    }
}
