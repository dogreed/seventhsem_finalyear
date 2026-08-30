using FinalProject_SeventhSem.Application.Common.Settings;
using FinalProject_SeventhSem.Application.Interfaces;
using FinalProject_SeventhSem.Application.Models.Ranking;
using FinalProject_SeventhSem.Domain.Entities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure.Engines;

/// <summary>
/// Implements IScoringService.
///
/// Algorithm 12 — Weighted Multi-Factor Scoring:
///
///   RankingScore = RequirementFit
///                + (LatestTestScore × AptitudeBonusWeight)   // max 30
///                + (OptionalFit × OptionalSkillWeight)        // max 15
///                + EducationBonus                             // max 5
///                + ProfileBonus                               // max 10
///
///   FinalScore = (RankingScore / MaxRankingScore) × 100
///
/// Denominator = 160 (documented in ThresholdSettings.MaxRankingScore).
/// Profile Bonus: GitHub +4, Portfolio +4, LinkedIn +2.
/// </summary>
public class ScoringEngine : IScoringService
{
    private readonly ThresholdSettings _t;

    public ScoringEngine(IOptions<ThresholdSettings> thresholds)
        => _t = thresholds.Value;

    public RankedCandidateDto Score(
        Student student,
        ApplicationMatchSnapshot snapshot,
        double latestTestScore,
        string applicationStatus,
        int rank)
    {
        double profileBonus = 0;
        if (!string.IsNullOrWhiteSpace(student.GitHubUrl)) profileBonus += _t.GitHubBonus;
        if (!string.IsNullOrWhiteSpace(student.PortfolioUrl)) profileBonus += _t.PortfolioBonus;
        if (!string.IsNullOrWhiteSpace(student.LinkedInUrl)) profileBonus += _t.LinkedInBonus;

        double aptitudeScore = Math.Round(latestTestScore * _t.AptitudeBonusWeight, 4);
        double optionalScore = Math.Round(snapshot.OptionalFit * _t.OptionalSkillWeight, 4);

        double rankingScore = snapshot.RequirementFit
                              + aptitudeScore
                              + optionalScore
                              + snapshot.EducationBonus
                              + profileBonus;

        double finalScore = Math.Round(rankingScore / _t.MaxRankingScore * 100, 2);

        return new RankedCandidateDto(
            Rank: rank,
            StudentId: student.Id,
            StudentName: student.FullName,
            PhotoUrl: student.PhotoUrl,
            GitHubUrl: student.GitHubUrl,
            PortfolioUrl: student.PortfolioUrl,
            LinkedInUrl: student.LinkedInUrl,
            RequirementFit: snapshot.RequirementFit,
            AptitudeScore: aptitudeScore,
            OptionalFitScore: optionalScore,
            EducationBonus: snapshot.EducationBonus,
            ProfileBonus: profileBonus,
            RankingScore: Math.Round(rankingScore, 4),
            FinalScore: finalScore,
            ApplicationStatus: applicationStatus);
    }
}

