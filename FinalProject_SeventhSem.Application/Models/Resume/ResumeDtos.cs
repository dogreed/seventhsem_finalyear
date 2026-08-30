using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Models.Resume;

public record ResumeParseResponse(
    IReadOnlyList<SkillSuggestion> Suggestions,
    int TotalExtracted
);

/// <summary>
/// A single skill suggestion from Algorithm 2 (Dictionary-Based NER).
/// Confidence: 0.9 for exact match, 0.7 for alias match.
/// </summary>
public record SkillSuggestion(
    int SkillId,
    string SkillName,
    double Confidence,
    string MatchType  
);


public record ConfirmResumeSkillsRequest(
    IReadOnlyList<int> ConfirmedSkillIds
);
