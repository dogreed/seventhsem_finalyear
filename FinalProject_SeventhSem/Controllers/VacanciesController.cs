using FinalProject_SeventhSem.Application.Features.Vacancies.Commands.CreateVacancy;
using FinalProject_SeventhSem.Application.Features.Vacancies.Commands.DeleteVacancy;
using FinalProject_SeventhSem.Application.Features.Vacancies.Commands.PublishVacancy;
using FinalProject_SeventhSem.Application.Features.Vacancies.Commands.UpdateVacancy;
using FinalProject_SeventhSem.Application.Features.Vacancies.Queries.GetOrganizationVacancies;
using FinalProject_SeventhSem.Application.Features.Vacancies.Queries.GetVacancy;
using FinalProject_SeventhSem.Application.Features.Vacancies.Queries.GetVacancyById;
using FinalProject_SeventhSem.Application.Features.Vacancies.Queries.GetVacancyMatches;
using FinalProject_SeventhSem.Application.Models.Vacancies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinalProject_SeventhSem.Controllers;

public class VacanciesController : ApiController
{
    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);


    /// <summary>All published vacancies with match scores for the student (Algorithms 3–6).</summary>
    [HttpGet("matches")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(IReadOnlyList<VacancyMatchResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMatches(CancellationToken ct)
        => Ok(await Sender.Send(new GetVacancyMatchesQuery(CurrentUserId), ct));

    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(VacancyResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => Ok(await Sender.Send(new GetVacancyByIdQuery(id), ct));



    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(VacancyResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllVacancy(CancellationToken ct)
        => Ok(await Sender.Send(new GetVacancyQuery(),ct));



    [HttpGet("mine")]
    [Authorize(Roles = "Organization")]
    [ProducesResponseType(typeof(IReadOnlyList<VacancyResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(CancellationToken ct)
        => Ok(await Sender.Send(new GetOrganizationVacanciesQuery(CurrentUserId), ct));


    [HttpPost]
    [Authorize(Roles = "Organization")]
    [ProducesResponseType(typeof(VacancyResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateVacancyRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new CreateVacancyCommand(
            OrganizationId: CurrentUserId,
            Title: request.Title,
            Description: request.Description,
            ApplicationDeadline: request.ApplicationDeadline,   
            RequiredEducationLevel: request.RequiredEducationLevel,
            RequiredFieldOfStudy: request.RequiredFieldOfStudy,
            RequiredSkillIds: request.RequiredSkillIds,
            OptionalSkillIds: request.OptionalSkillIds), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.VacancyId }, result);
    }

    /// <summary>Update a vacancy draft. Cannot edit a published vacancy.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organization")]
    [ProducesResponseType(typeof(VacancyResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateVacancyRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new UpdateVacancyCommand(
            VacancyId: id,
            OrganizationId: CurrentUserId,
            Title: request.Title,
            Description: request.Description,
            ApplicationDeadline: request.ApplicationDeadline,  
            RequiredEducationLevel: request.RequiredEducationLevel,
            RequiredFieldOfStudy: request.RequiredFieldOfStudy,
            RequiredSkillIds: request.RequiredSkillIds,
            OptionalSkillIds: request.OptionalSkillIds), ct);
        return Ok(result);
    }

 


    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organization")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await Sender.Send(new DeleteVacancyCommand(id, CurrentUserId), ct);
        return NoContent();
    }
}
