using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Models.Ranking;

public record RankedCandidateListResponse(
    int VacancyId,
    string VacancyTitle,
    IReadOnlyList<RankedCandidateDto> Candidates
);

/// <summary>
/// Full score breakdown for a single candidate.
/// FinalScore = (RankingScore / 160) * 100  — normalized to 0–100.
/// </summary>
public record RankedCandidateDto(
    int Rank,
    int StudentId,
    string StudentName,
    string? PhotoUrl,
    string? GitHubUrl,
    string? PortfolioUrl,
    string? LinkedInUrl,

    double RequirementFit,      // 0–100  (weight: 1×)
    double AptitudeScore,       // LatestTestScore * AptitudeBonusWeight (0.30)
    double OptionalFitScore,    // OptionalFit * OptionalSkillWeight (0.15)
    double EducationBonus,      // 0 or 5
    double ProfileBonus,        // 0–10 (GitHub +4, Portfolio +4, LinkedIn +2)

    double RankingScore,        // Sum of above components (max 160)
    double FinalScore,          // (RankingScore / 160) * 100

    string ApplicationStatus
);
