using FinalProject_SeventhSem.Application.Features.Resume.Commands.UploadResume;
using FinalProject_SeventhSem.Application.Features.Students.Commands.SetStudentSkills;
using FinalProject_SeventhSem.Application.Models.Resume;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinalProject_SeventhSem.Controllers;

/// <summary>
/// Resume upload (PDF only) and skill confirmation.
/// Rate-limited to 5 uploads/hour via AspNetCoreRateLimit (configured in Program.cs).
/// </summary>
[Authorize(Roles = "Student")]
public class ResumeController : ApiController
{
    private int CurrentUserId =>
       int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Upload PDF resume. Returns skill suggestions (Algorithms 1+2). Rate-limited: 5/hour.</summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ResumeParseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });
        await using var stream = file.OpenReadStream();
        var result = await Sender.Send(
            new UploadResumeCommand(CurrentUserId, stream, file.FileName), ct);
        return Ok(result);
    }

}
