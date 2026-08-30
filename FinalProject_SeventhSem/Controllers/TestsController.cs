using FinalProject_SeventhSem.Application.Features.Tests.Commands.StartTest;
using FinalProject_SeventhSem.Application.Features.Tests.Commands.SubmitAnswer;
using FinalProject_SeventhSem.Application.Features.Tests.Commands.SubmitTest;
using FinalProject_SeventhSem.Application.Features.Tests.Queries.GetTestHistory;
using FinalProject_SeventhSem.Application.Models.Tests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinalProject_SeventhSem.Controllers;

/// <summary>
/// Aptitude test lifecycle: start → answer → submit.
///
/// Start    → Algorithm 7  (Round-Robin Question Selection)
/// Submit   → Algorithm 8  (Test Scoring)
///          + Algorithm 9  (Chapter Analysis)
///          + Algorithm 10 (Weak Chapter Detection)
///          + Algorithm 11 (Resource Recommendation)
///
/// Rate-limited: 3 submissions/hour via AspNetCoreRateLimit.
/// Time limit enforced per-answer: auto-submits if ExpiresAt exceeded.
/// </summary>
[Authorize(Roles = "Student")]
public class TestsController : ApiController
{
    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);


    /// <summary>Start a new test session — Algorithm 7 (Round-Robin Question Selection).</summary>
    [HttpPost("start")]
    [ProducesResponseType(typeof(TestSessionResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Start([FromBody] StartTestRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new StartTestCommand(CurrentUserId, request.StackId), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Submit a single answer. Auto-submits test if time limit exceeded.</summary>
    [HttpPost("{testId:int}/answers")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SubmitAnswer(
        int testId, [FromBody] SubmitAnswerRequest request, CancellationToken ct)
    {
        await Sender.Send(new SubmitAnswerCommand(
            testId, CurrentUserId, request.QuestionId, request.SelectedOption), ct);
        return NoContent();
    }

    /// <summary>Explicitly submit test. Triggers Algorithms 8–11. Rate-limited: 3/hour.</summary>
    [HttpPost("{testId:int}/submit")]
    [ProducesResponseType(typeof(TestResultResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Submit(int testId, CancellationToken ct)
    {
        var result = await Sender.Send(new SubmitTestCommand(testId, CurrentUserId), ct);
        return Ok(result);
    }

    /// <summary>Get all past test results for the authenticated student. IsLatest marks the ranking-used score.</summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(IReadOnlyList<TestHistoryItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(CancellationToken ct)
    {
        var result = await Sender.Send(new GetTestHistoryQuery(CurrentUserId), ct);
        return Ok(result);
    }
}

public record StartTestRequest(int StackId);
