using FinalProject_SeventhSem.Application.Features.Admin.Queries.GetOrganizationDashboard;
using FinalProject_SeventhSem.Application.Features.Applications.Commands.ApplyToVacancy;
using FinalProject_SeventhSem.Application.Features.Applications.Commands.UpdateApplicationStatus;
using FinalProject_SeventhSem.Application.Features.Applications.Queries.GetOrganizationApplications;
using FinalProject_SeventhSem.Application.Features.Applications.Queries.GetRankedCandidates;
using FinalProject_SeventhSem.Application.Features.Applications.Queries.GetStudentApplications;
using FinalProject_SeventhSem.Application.Models.Applications;
using FinalProject_SeventhSem.Application.Models.Ranking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinalProject_SeventhSem.Controllers;

public class ApplicationsController : ApiController
{
    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Apply(
        [FromBody] ApplyToVacancyRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(
            new ApplyToVacancyCommand(CurrentUserId, request.VacancyId), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    //[HttpGet("dashboard")]
    //[Authorize(Roles = "Organization")]
    //[ProducesResponseType(typeof(OrganizationDashboardResponse), StatusCodes.Status200OK)]
    //public async Task<IActionResult> GetDashboard(CancellationToken ct)
    //    => Ok(await Sender.Send(new GetOrganizationDashboardQuery(), ct));

    [HttpGet("mine")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(IReadOnlyList<ApplicationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyApplications(CancellationToken ct)
    {
        var result = await Sender.Send(new GetStudentApplicationsQuery(CurrentUserId), ct);
        return Ok(result);
    }

    [HttpGet("vacancies/{vacancyId:int}/ranked")]
    [Authorize(Roles = "Organization")]
    [ProducesResponseType(typeof(RankedCandidateListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRankedCandidates(int vacancyId, CancellationToken ct)
    {
        var result = await Sender.Send(
            new GetRankedCandidatesQuery(vacancyId, CurrentUserId), ct);
        return Ok(result);
    }


    [HttpGet("applications")]
    [Authorize(Roles = "Organization")]
    [ProducesResponseType(typeof(IReadOnlyList<OrganizationApplicationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganizationApplications(CancellationToken ct)
    {
        var result = await Sender.Send(new GetOrganizationApplicationsQuery(CurrentUserId), ct);
        return Ok(result);
    }

    [HttpGet("vacancies/{vacancyId:int}/applications")]
    [Authorize(Roles = "Organization")]
    [ProducesResponseType(typeof(IReadOnlyList<OrganizationApplicationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetApplicationsByVacancy(int vacancyId, CancellationToken ct)
    {
        var result = await Sender.Send(new GetApplicationsByVacancyQuery(vacancyId, CurrentUserId), ct);
        return Ok(result);
    }


    [HttpPatch("{applicationId:int}/status")]
    [Authorize(Roles = "Organization")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateStatus(
        int applicationId,
        [FromBody] UpdateApplicationStatusRequest request,
        CancellationToken ct)
    {
        await Sender.Send(new UpdateApplicationStatusCommand(
            applicationId, CurrentUserId, request.NewStatus), ct);
        return Ok("Application status updated successfully.");
    }
}
