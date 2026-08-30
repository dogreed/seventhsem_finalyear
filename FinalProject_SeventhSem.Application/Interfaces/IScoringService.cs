using FinalProject_SeventhSem.Application.Models.Ranking;
using FinalProject_SeventhSem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Interfaces;

public interface IScoringService
{
    /// <summary>
    /// Algorithm 12: computes RankingScore then normalises to FinalScore.
    ///
    /// Formula:
    ///   RankingScore = RequirementFit
    ///                + (LatestTestScore * AptitudeBonusWeight)
    ///                + (OptionalFit * OptionalSkillWeight)
    ///                + EducationBonus
    ///                + ProfileBonus
    ///
    ///   FinalScore = (RankingScore / 160) * 100
    /// </summary>
    RankedCandidateDto Score(
        Student student,
        ApplicationMatchSnapshot snapshot,
        double latestTestScore,
        string applicationStatus,
        int rank);
}

