using FinalProject_SeventhSem.Application.Features.Admin.Commands.ManageChapter;
using FinalProject_SeventhSem.Application.Features.Admin.Commands.ManageQuestion;
using FinalProject_SeventhSem.Application.Features.Admin.Commands.ManageResource;
using FinalProject_SeventhSem.Application.Features.Admin.Commands.ManageSkill;
using FinalProject_SeventhSem.Application.Features.Admin.Commands.ManageStack;
using FinalProject_SeventhSem.Application.Features.Admin.Commands.VerifyOrganization;
using FinalProject_SeventhSem.Application.Features.Admin.Queries.GetAdminDashboard;
using FinalProject_SeventhSem.Application.Features.Admin.Queries.GetAllOrganizations;
using FinalProject_SeventhSem.Application.Features.Admin.Queries.GetAllStudents;
using FinalProject_SeventhSem.Application.Models.Skills;
using FinalProject_SeventhSem.Application.Models.Stacks;
using FinalProject_SeventhSem.Domain.Enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace FinalProject_SeventhSem.Controllers;


[Authorize(Roles = "Admin")]
public class AdminController : ApiController
{

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(AdminDashboardSummaryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummaryStats(CancellationToken ct)
        => Ok(await Sender.Send(new GetAdminDashboardSummaryQuery(), ct));

    [HttpGet("students")]
    [ProducesResponseType(typeof(IReadOnlyList<StudentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudents(CancellationToken ct = default)
    => Ok(await Sender.Send(new GetAllStudentsQuery(), ct));

    [HttpGet("organizations")]
    [ProducesResponseType(typeof(IReadOnlyList<OrganizationSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizations(
        [FromQuery] bool pendingOnly = false, CancellationToken ct = default)
        => Ok(await Sender.Send(new GetAllOrganizationsQuery(pendingOnly), ct));

    //[HttpPost("organizations/{organizationId:int}/verify")]
    //[ProducesResponseType(typeof(VerifyOrganizationResponse), StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status404NotFound)]
    //public async Task<IActionResult> VerifyOrganization(int organizationId, CancellationToken ct)
    //{
    //   var result =  await Sender.Send(new VerifyOrganizationCommand(organizationId), ct);
    //    return Ok(result);
    //}


    
    public record VerifyOrganizationRequest(OrganizationStatus Status, string? Reason = null);

    [HttpPut("organizations/{organizationId:int}/status")]
    [ProducesResponseType(typeof(VerifyOrganizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrganizationStatus(
    int organizationId,
    [FromBody] VerifyOrganizationRequest request,
    CancellationToken ct)
    {
        var result = await Sender.Send(
            new VerifyOrganizationCommand(organizationId, request.Status, request.Reason), ct);
        return Ok(result);
    }

    [HttpPost("skills")]
    [ProducesResponseType(typeof(SkillResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSkill(
        [FromBody] CreateSkillRequest request, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created,
            await Sender.Send(new CreateSkillCommand(request.Name), ct));

    [HttpPost("skills/aliases")]
    [ProducesResponseType(typeof(SkillAliasResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSkillAlias(
        [FromBody] CreateSkillAliasRequest request, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created,
            await Sender.Send(new CreateSkillAliasCommand(request.SkillId, request.Alias), ct));

    [HttpDelete("skills/aliases/{aliasId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteSkillAlias(int aliasId, CancellationToken ct)
    {
        await Sender.Send(new DeleteSkillAliasCommand(aliasId), ct);
        return NoContent();
    }

    [HttpGet("stacks")]
    [ProducesResponseType(typeof(IReadOnlyList<StackResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStacks(CancellationToken ct)
        => Ok(await Sender.Send(new GetAllStacksQuery(), ct));

    [HttpPost("stacks")]
    [ProducesResponseType(typeof(StackResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateStack(
        [FromBody] CreateStackRequest request, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created,
            await Sender.Send(new CreateStackCommand(request.Name), ct));

    [HttpPut("stacks/{stackId:int}")]
    [ProducesResponseType(typeof(StackResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStack(
        int stackId, [FromBody] UpdateStackRequest request, CancellationToken ct)
        => Ok(await Sender.Send(new UpdateStackCommand(stackId, request.Name), ct));

    [HttpDelete("stacks/{stackId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteStack(int stackId, CancellationToken ct)
    {
        await Sender.Send(new DeleteStackCommand(stackId), ct);
        return NoContent();
    }

    [HttpGet("stacks/{stackId:int}/chapters")]
    [ProducesResponseType(typeof(IReadOnlyList<ChapterResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChapters(int stackId, CancellationToken ct)
        => Ok(await Sender.Send(new GetChaptersByStackQuery(stackId), ct));

    [HttpPost("chapters")]
    [ProducesResponseType(typeof(ChapterResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateChapter(
        [FromBody] CreateChapterRequest request, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created,
            await Sender.Send(new CreateChapterCommand(request.StackId, request.Name), ct));

    [HttpPut("chapters/{chapterId:int}")]
    [ProducesResponseType(typeof(ChapterResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateChapter(
        int chapterId, [FromBody] UpdateChapterRequest request, CancellationToken ct)
        => Ok(await Sender.Send(new UpdateChapterCommand(chapterId, request.Name), ct));

    [HttpDelete("chapters/{chapterId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteChapter(int chapterId, CancellationToken ct)
    {
        await Sender.Send(new DeleteChapterCommand(chapterId), ct);
        return NoContent();
    }



    [HttpGet("stacks/{stackId:int}/chapters/{chapterId:int}/questions")]
    [ProducesResponseType(typeof(IReadOnlyList<QuestionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuestionsByChapter(int stackId, int chapterId, CancellationToken ct)
        => Ok(await Sender.Send(new GetQuestionsByChapterQuery(stackId, chapterId), ct));

    [HttpGet("stacks/{stackId:int}/questions")]
    [ProducesResponseType(typeof(IReadOnlyList<QuestionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuestionsByStack(int stackId, CancellationToken ct)
        => Ok(await Sender.Send(new GetQuestionsByStackQuery(stackId), ct));



    [HttpPost("questions")]
    [ProducesResponseType(typeof(QuestionResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateQuestion(
    [FromBody] CreateQuestionRequest request, CancellationToken ct)
    => StatusCode(StatusCodes.Status201Created,
        await Sender.Send(new CreateQuestionCommand(
            request.StackId, request.ChapterId, request.Text, request.OptionA, request.OptionB,
            request.OptionC, request.OptionD, request.CorrectOption), ct));

    [HttpPatch("questions/{questionId:int}")]
    [ProducesResponseType(typeof(QuestionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> PatchQuestion(
    int questionId, [FromBody] PatchQuestionRequest request, CancellationToken ct)
    => Ok(await Sender.Send(new PatchQuestionCommand(
        questionId, request.Text, request.OptionA, request.OptionB,
        request.OptionC, request.OptionD, request.CorrectOption), ct));


    [HttpDelete("questions/{questionId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteQuestion(int questionId, CancellationToken ct)
    {
        await Sender.Send(new DeleteQuestionCommand(questionId), ct);
        return Ok($"{questionId} deleted successfully");
    }


    


    //[HttpGet("resources")]
    //[ProducesResponseType(typeof(IReadOnlyList<ResourceDto>), StatusCodes.Status200OK)]
    //public async Task<IActionResult> GetResources(CancellationToken ct)
    //    => Ok(await Sender.Send(new GetAllResourcesQuery(), ct));

    //[HttpPost("resources")]
    //[ProducesResponseType(typeof(ResourceDto), StatusCodes.Status201Created)]
    //public async Task<IActionResult> CreateResource(
    //    [FromBody] CreateResourceCommand request, CancellationToken ct)
    //    => StatusCode(StatusCodes.Status201Created,
    //        await Sender.Send(request, ct));

    //[HttpPut("resources/{resourceId:int}")]
    //[ProducesResponseType(typeof(ResourceDto), StatusCodes.Status200OK)]
    //public async Task<IActionResult> UpdateResource(
    //    int resourceId, [FromBody] UpdateResourceCommand request, CancellationToken ct)
    //    => Ok(await Sender.Send(request with { ResourceId = resourceId }, ct));

    //[HttpDelete("resources/{resourceId:int}")]
    //[ProducesResponseType(StatusCodes.Status204NoContent)]
    //public async Task<IActionResult> DeleteResource(int resourceId, CancellationToken ct)
    //{
    //    await Sender.Send(new DeleteResourceCommand(resourceId), ct);
    //    return NoContent();
    //}
}
