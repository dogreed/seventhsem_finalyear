using FinalProject_SeventhSem.Application.Common;
using FinalProject_SeventhSem.Application.Common.Settings;
using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Models.Tests;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Enums;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Tests.Commands.StartTest;


/// <summary>
/// Algorithm 7 — Random Non-Repeating Round-Robin Question Selection.
///
/// Steps:
///  1. Fetch all chapters with their questions.
///  2. Fetch seenQuestionIds for this student.
///  3. Loop round-robin over chapters:
///       - availableQuestions = chapter.Questions EXCEPT seenQuestionIds
///       - Pick ONE random question from available pool
///       - Track in-memory to prevent intra-test duplicates
///  4. Repeat until TotalQuestions reached or all chapters exhausted.
///  5. Create Test entity. Bulk-insert seen questions on submission (not here).
/// </summary>
public class StartTestCommandHandler : IRequestHandler<StartTestCommand, TestSessionResponse>
{
    private readonly IRepository<Student> _studentRepo;
    private readonly IRepository<Chapter> _chapterRepo;
    private readonly IRepository<Stack> _stackRepo;
    private readonly IRepository<StudentSeenQuestion> _seenRepo;
    private readonly IRepository<Test> _testRepo;
    private readonly IUnitOfWork _uow;
    private readonly TestSettings _settings;

    public StartTestCommandHandler(
        IRepository<Student> studentRepo,
        IRepository<Chapter> chapterRepo,
        IRepository<Stack> stackRepo,
        IRepository<StudentSeenQuestion> seenRepo,
        IRepository<Test> testRepo,
        IUnitOfWork uow,
        IOptions<TestSettings> settings)
    {
        _studentRepo = studentRepo;
        _chapterRepo = chapterRepo;
        _stackRepo = stackRepo;
        _seenRepo = seenRepo;
        _testRepo = testRepo;
        _uow = uow;
        _settings = settings.Value;
    }

    public async Task<TestSessionResponse> Handle(
        StartTestCommand request, CancellationToken cancellationToken)
    {
        var student = await StudentResolver.ResolveAsync(request.StudentId, _studentRepo, cancellationToken);


        var stack = await _stackRepo.GetByIdAsync(request.StackId, cancellationToken)
            ?? throw new NotFoundException(nameof(Stack), request.StackId);
        // Fetch seen question IDs for this student
        var seenIds = (await _seenRepo.GetAllAsync(cancellationToken))
            .Where(s => s.StudentId == request.StudentId)
            .Select(s => s.QuestionId)
            .ToHashSet();

        //var chapters = (await _chapterRepo.GetAllAsync(cancellationToken))
        //    .Where(c => c.Questions.Any())
        //    .ToList();

        //var chapters = await _chapterRepo.GetAllAsync(
        //   q => q
        //       .Include(c => c.Questions)
        //       .Include(c => c.Stack)
        //       .Where(c => c.Questions.Any()),
        //   cancellationToken);


        var chapters = await _chapterRepo.GetAllAsync(
         q => q
             .Include(c => c.Questions)
             .Include(c => c.Stack)
             .Where(c => c.StackId == request.StackId && c.Questions.Any()),
         cancellationToken);

        if (!chapters.Any())
            throw new BadRequestException("No questions are available for a test session.");

        var selected = new List<Question>();
        var inMemorySeen = new HashSet<int>(seenIds);
        var rng = new Random();
        int total = _settings.QuestionsPerTest;

        // Round-robin over chapters until we have enough or all are exhausted
        bool anyAvailable = true;
        while (selected.Count < total && anyAvailable)
        {
            anyAvailable = false;
            foreach (var chapter in chapters)
            {
                if (selected.Count >= total) break;

                var available = chapter.Questions
                    .Where(q => !inMemorySeen.Contains(q.Id))
                    .ToList();

                if (!available.Any()) continue;

                anyAvailable = true;
                var pick = available[rng.Next(available.Count)];
                selected.Add(pick);
                inMemorySeen.Add(pick.Id);
            }
        }

        if (!selected.Any())
            throw new BadRequestException("All available questions have already been shown to this student.");

        // Create test session
        var now = DateTime.UtcNow;
        var test = new Test
        {
            StudentId = student.Id,
            StartedAt = now,
            ExpiresAt = now.AddMinutes(_settings.TestDurationMinutes),
            Status = TestStatus.InProgress
        };
        await _testRepo.AddAsync(test, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new TestSessionResponse(
            TestId: test.Id,
            StartedAt: test.StartedAt,
            ExpiresAt: test.ExpiresAt,
            Questions: selected.Select(q => new TestQuestionDto(
                QuestionId: q.Id,
                Text: q.Text,
                OptionA: q.OptionA,
                OptionB: q.OptionB,
                OptionC: q.OptionC,
                OptionD: q.OptionD,
                ChapterName: q.Chapter.Name,
                StackName: q.Chapter.Stack.Name)).ToList());
    }
}
