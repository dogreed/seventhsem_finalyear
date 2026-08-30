using FinalProject_SeventhSem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Models.Vacancies;

public record CreateVacancyRequest(
    string Title,
    string Description,
     DateTime ApplicationDeadline,
    EducationLevel? RequiredEducationLevel,
    string? RequiredFieldOfStudy,
    IReadOnlyList<int> RequiredSkillIds,   
    IReadOnlyList<int>? OptionalSkillIds
);

public record UpdateVacancyRequest(
    string Title,
    string Description,
    DateTime ApplicationDeadline,
    EducationLevel? RequiredEducationLevel,
    string? RequiredFieldOfStudy,
    IReadOnlyList<int> RequiredSkillIds,
    IReadOnlyList<int>? OptionalSkillIds
);


public record VacancyResponse(
    int VacancyId,
    int OrganizationId,
    string OrganizationName,
    string Title,
    string Description,
    bool IsPublished,
    
    DateTime? PublishedAt,
    DateTime? ApplicationDeadline,
    bool IsDeadlinePassed,             
    int? DaysRemaining,
    string? RequiredEducationLevel,
    string? RequiredFieldOfStudy,
    IReadOnlyList<VacancySkillDto> RequiredSkills,
    IReadOnlyList<VacancySkillDto> OptionalSkills
);

public record VacancySkillDto(
    int SkillId,
    string SkillName
);

public record VacancyMatchResult(
    int VacancyId,
    string Title,
    string OrganizationName,
    bool IsEligible,
    string? EligibilityMessage,        // populated when IsEligible = false
    double RequirementFit,             // Algorithm 4 output (0–100)
    double OptionalFit,                // Algorithm 5 output (0–100)
    double EducationBonus,             // Algorithm 3 output (0 or 5)
    IReadOnlyList<string> MissingSkills, // Algorithm 6 output (flat list)
     IReadOnlyList<int> MissingSkillIds);
