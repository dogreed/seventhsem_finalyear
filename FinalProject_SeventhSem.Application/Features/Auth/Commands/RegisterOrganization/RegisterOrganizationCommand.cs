using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Interfaces;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Enums;
using FinalProject_SeventhSem.Domain.Interfaces;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Auth.Commands.RegisterOrganization;

public record RegisterOrganizationCommand(
    string OrganizationName,
    string Email,
    string Password,
    string? WebsiteUrl
) : IRequest<string>; // returns a simple confirmation message

// ── Validator ─────────────────────────────────────────────────────────────────

