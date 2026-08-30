using FinalProject_SeventhSem.Application.Features.Admin.Commands.ManageStack;
using FinalProject_SeventhSem.Application.Features.Students.Commands.SetStudentSkills;
using FinalProject_SeventhSem.Application.Features.Students.Commands.UpdateStudentProfile;
using FinalProject_SeventhSem.Application.Features.Students.Commands.UploadProfilePhoto;
using FinalProject_SeventhSem.Application.Features.Students.Queries.GetStudentDashboard;
using FinalProject_SeventhSem.Application.Features.Students.Queries.GetStudentProfile;
using FinalProject_SeventhSem.Application.Models.Stacks;
using FinalProject_SeventhSem.Application.Models.Students;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinalProject_SeventhSem.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : ApiController
{
    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("me")]
    [ProducesResponseType(typeof(StudentProfileResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
        => Ok(await Sender.Send(new GetStudentProfileQuery(CurrentUserId), ct));

    [HttpPut("me")]
    [ProducesResponseType(typeof(StudentProfileResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateStudentProfileRequest request, CancellationToken ct)
    {
        await Sender.Send(new UpdateStudentProfileCommand(
            UserId: CurrentUserId, FullName: request.FullName,
            PhoneNumber: request.PhoneNumber, Bio: request.Bio,
            Nationality: request.Nationality, Location: request.Location,
            EducationLevel: request.EducationLevel, FieldOfStudy: request.FieldOfStudy,
            GitHubUrl: request.GitHubUrl, PortfolioUrl: request.PortfolioUrl,
            LinkedInUrl: request.LinkedInUrl), ct);
        var profile = await Sender.Send(new GetStudentProfileQuery(CurrentUserId), ct);
        return Ok(profile);
    }

    [HttpGet("me/dashboard")]
    [ProducesResponseType(typeof(ProfileCompletenessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
        => Ok(await Sender.Send(new GetStudentDashboardQuery(CurrentUserId), ct));

    [HttpPut("me/skills")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetSkills(
        [FromBody] IReadOnlyList<int> confirmedSkillIds, CancellationToken ct)
    {
        await Sender.Send(new SetStudentSkillsCommand(CurrentUserId, confirmedSkillIds), ct);
        return NoContent();
    }

    /// <summary>Upload a profile photo (JPG, PNG, WEBP — max 5 MB).</summary>
    [HttpPost("me/photo")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPhoto(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });
        await using var stream = file.OpenReadStream();
        var url = await Sender.Send(
            new UploadProfilePhotoCommand(CurrentUserId, stream, file.FileName), ct);
        return Ok(new { photoUrl = url });
    }


    [HttpGet("stacks")]
    [ProducesResponseType(typeof(IReadOnlyList<StackResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStacks(CancellationToken ct)
       => Ok(await Sender.Send(new GetAllStacksQuery(), ct));
}
