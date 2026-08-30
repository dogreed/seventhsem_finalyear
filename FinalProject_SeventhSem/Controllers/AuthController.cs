using FinalProject_SeventhSem.Application.Features.Auth.Commands.Login;
using FinalProject_SeventhSem.Application.Features.Auth.Commands.RefreshToken;
using FinalProject_SeventhSem.Application.Features.Auth.Commands.RegisterOrganization;
using FinalProject_SeventhSem.Application.Features.Auth.Commands.RegisterStudent;
using FinalProject_SeventhSem.Application.Models.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject_SeventhSem.Controllers;

public class AuthController : ApiController
{
    [HttpPost("register/student")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterStudent(
        [FromBody] RegisterStudentRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(
            new RegisterStudentCommand(request.FullName, request.Email, request.Password), ct);
        return CreatedAtAction(nameof(RegisterStudent), result);
    }

    [HttpPost("register/organization")]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterOrganization(
        [FromBody] RegisterOrganizationRequest request,
        CancellationToken ct)
    {
        var message = await Sender.Send(
            new RegisterOrganizationCommand(
                request.OrganizationName, request.Email,
                request.Password, request.WebsiteUrl), ct);
        return StatusCode(StatusCodes.Status201Created, new { message });
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(new LoginCommand(request.Email, request.Password), ct);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(new RefreshTokenCommand(request.RefreshToken), ct);
        return Ok(result);
    }
}
