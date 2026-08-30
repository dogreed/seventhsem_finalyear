using FinalProject_SeventhSem.Application.Models.Vacancies;
using FinalProject_SeventhSem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Interfaces;

public interface IMatchingService
{
    /// <summary>
    /// Algorithm 3: Education Check (rule-based hard filter).
    /// Returns (isEligible, educationBonus).
    /// </summary>
    (bool IsEligible, double EducationBonus) CheckEducation(Student student, Vacancy vacancy);

    /// <summary>
    /// Algorithm 4: Requirement Fit = (matched ∩ required) / |required| * 100.
    /// </summary>
    double ComputeRequirementFit(IEnumerable<int> studentSkillIds, IEnumerable<int> requiredSkillIds);

    /// <summary>
    /// Algorithm 5: Optional Fit — same ratio applied to optional skills.
    /// </summary>
    double ComputeOptionalFit(IEnumerable<int> studentSkillIds, IEnumerable<int> optionalSkillIds);

    /// <summary>
    /// Algorithm 6: Skill Gap = (required ∪ optional) − student skills.
    /// Returns flat list of missing SkillIds.
    /// </summary>
    IReadOnlyList<int> ComputeSkillGap(
        IEnumerable<int> studentSkillIds,
        IEnumerable<int> requiredSkillIds,
        IEnumerable<int> optionalSkillIds);

    /// <summary>
    /// Runs Algorithms 3–6 for one student against one vacancy and returns the full result.
    /// </summary>
    VacancyMatchResult Match(Student student, Vacancy vacancy);
}
