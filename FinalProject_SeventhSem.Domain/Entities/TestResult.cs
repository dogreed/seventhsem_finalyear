using FinalProject_SeventhSem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Domain.Entities;

public class TestResult : BaseEntity
{
    public int TestId { get; set; }
    public int StudentId { get; set; }

    /// <summary>Output of Algorithm 8: (CorrectAnswers / TotalAnswered) * 100.</summary>
    public double Score { get; set; }

    public int TotalAnswered { get; set; }
    public int CorrectAnswers { get; set; }

    /// <summary>JSON object of per-chapter scores. Output of Algorithm 9.</summary>
    public string ChapterScoresJson { get; set; } = "{}";

    /// <summary>JSON array of weak ChapterIds. Output of Algorithm 10.</summary>
    public string WeakChapterIdsJson { get; set; } = "[]";

    /// <summary>True for the most recent test result per student (used by Algorithm 12).</summary>
    public bool IsLatest { get; set; } = true;

    public DateTime ComputedAt { get; set; } = DateTime.UtcNow;

    public Test Test { get; set; } = null!;
}