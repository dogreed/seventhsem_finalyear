using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Models.Tests;

public record SubmitAnswerRequest(
    int TestId,
    int QuestionId,
    string SelectedOption   
);

public record SubmitTestRequest(
    int TestId
);

public record TestSessionResponse(
    int TestId,
    DateTime StartedAt,
    DateTime ExpiresAt,
    IReadOnlyList<TestQuestionDto> Questions
);

public record TestQuestionDto(
    int QuestionId,
    string Text,
    string OptionA,
    string OptionB,
    string OptionC,
    string OptionD,
    string ChapterName,
    string StackName
);

public record TestResultResponse(
    int TestId,
    double Score,                                       // Algorithm 8
    int TotalAnswered,
    int CorrectAnswers,
    IReadOnlyList<ChapterScoreDto> ChapterScores,       // Algorithm 9
    IReadOnlyList<string> WeakChapters,                 // Algorithm 10
    IReadOnlyList<ResourceRecommendationDto> RecommendedResources // Algorithm 11
);

public record ChapterScoreDto(
    int ChapterId,
    string ChapterName,
    string StackName,
    double ScorePercent,
    bool IsWeak            
);

/// <summary>A single resource recommended by Algorithm 11.</summary>
public record ResourceRecommendationDto(
    int ResourceId,
    string Title,
    string Url,
    string? ResourceType,
    string RecommendedBecause  
);

