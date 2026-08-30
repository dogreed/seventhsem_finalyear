using FinalProject_SeventhSem.Application.Features.Resources.Commands.GetRecommendResources;
using FinalProject_SeventhSem.Application.Features.Resources.Commands.RateResource;
using FinalProject_SeventhSem.Application.Models.Tests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinalProject_SeventhSem.Controllers;


[Authorize(Roles = "Student")]
public class ResourcesController : ApiController
{
    private int CurrentStudentId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("recommended")]
    [ProducesResponseType(typeof(IReadOnlyList<ResourceRecommendationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecommended(CancellationToken ct)
    {
        var result = await Sender.Send(new GetRecommendedResourcesQuery(CurrentStudentId), ct);
        return Ok(result);
    }

    [HttpPost("{resourceId:int}/rate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Rate(
        int resourceId,
        [FromBody] RateRequest request,
        CancellationToken ct)
    {
        await Sender.Send(new RateResourceCommand(
            resourceId, CurrentStudentId, request.Rating, request.Comment), ct);
        return NoContent();
    }
}


public record RateRequest(int Rating, string? Comment);