using FinalProject_SeventhSem.Application.Common;
using FinalProject_SeventhSem.Application.Models.Tests;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Tests.Queries.GetTestHistory;

// ── Response DTO ──────────────────────────────────────────────────────────────

public record TestHistoryItemDto(
    int TestId,
    DateTime StartedAt,
    DateTime? SubmittedAt,
    string Status,
    double Score,
    int TotalAnswered,
    int CorrectAnswers,
    bool IsLatest,
    IReadOnlyList<ChapterScoreDto> ChapterScores,
    IReadOnlyList<string> WeakChapters
);

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Returns all past test results for the authenticated student,
/// ordered most-recent first. IsLatest=true marks the score used in ranking.
/// </summary>
public record GetTestHistoryQuery(int UserId) : IRequest<IReadOnlyList<TestHistoryItemDto>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public class GetTestHistoryQueryHandler
    : IRequestHandler<GetTestHistoryQuery, IReadOnlyList<TestHistoryItemDto>>
{
    private readonly IRepository<Student> _studentRepo;
    private readonly IRepository<TestResult> _testResultRepo;
    private readonly IRepository<Chapter> _chapterRepo;

    public GetTestHistoryQueryHandler(
        IRepository<Student> studentRepo,
        IRepository<TestResult> testResultRepo,
        IRepository<Chapter> chapterRepo)
    {
        _studentRepo = studentRepo;
        _testResultRepo = testResultRepo;
        _chapterRepo = chapterRepo;
    }

    public async Task<IReadOnlyList<TestHistoryItemDto>> Handle(
        GetTestHistoryQuery request, CancellationToken cancellationToken)
    {
        var student = await StudentResolver.ResolveAsync(request.UserId, _studentRepo, cancellationToken);
        var chapters = await _chapterRepo.GetAllAsync(cancellationToken);
        var chapterMap = chapters.ToDictionary(c => c.Id);

        var results = (await _testResultRepo.GetAllAsync(cancellationToken))
            .Where(tr => tr.StudentId == student.Id)
            .OrderByDescending(tr => tr.ComputedAt)
            .ToList();

        return results.Select(tr =>
        {
            // Deserialise chapter score JSON → { "chapterId": scorePercent }
            var chapterScores = new List<ChapterScoreDto>();
            try
            {
                var scoreDict = JsonSerializer
                    .Deserialize<Dictionary<string, double>>(tr.ChapterScoresJson) ?? [];

                var weakIds = JsonSerializer
                    .Deserialize<List<int>>(tr.WeakChapterIdsJson) ?? [];
                var weakSet = weakIds.ToHashSet();

                foreach (var (key, pct) in scoreDict)
                {
                    if (!int.TryParse(key, out var cid)) continue;
                    var chapter = chapterMap.GetValueOrDefault(cid);
                    if (chapter is null) continue;

                    chapterScores.Add(new ChapterScoreDto(
                        ChapterId: cid,
                        ChapterName: chapter.Name,
                        StackName: chapter.Stack.Name,
                        ScorePercent: pct,
                        IsWeak: weakSet.Contains(cid)));
                }
            }
            catch { /* malformed JSON — return empty */ }

            var weakChapterNames = chapterScores
                .Where(c => c.IsWeak)
                .Select(c => c.ChapterName)
                .ToList();

            return new TestHistoryItemDto(
                TestId: tr.TestId,
                StartedAt: tr.Test.StartedAt,
                SubmittedAt: tr.Test.SubmittedAt,
                Status: tr.Test.Status.ToString(),
                Score: tr.Score,
                TotalAnswered: tr.TotalAnswered,
                CorrectAnswers: tr.CorrectAnswers,
                IsLatest: tr.IsLatest,
                ChapterScores: chapterScores,
                WeakChapters: weakChapterNames);
        }).ToList();
    }
}

