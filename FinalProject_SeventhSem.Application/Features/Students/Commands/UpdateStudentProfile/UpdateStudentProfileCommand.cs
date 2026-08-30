using FinalProject_SeventhSem.Application.Exceptions;
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

namespace FinalProject_SeventhSem.Application.Features.Students.Commands.UpdateStudentProfile;

public record UpdateStudentProfileCommand(
    int UserId,
    string FullName,
    string? PhoneNumber,
    string? Bio,
    string? Nationality,
    string? Location,
    EducationLevel? EducationLevel,
    string? FieldOfStudy,
    string? GitHubUrl,
    string? PortfolioUrl,
    string? LinkedInUrl
) : IRequest;

// ── Validator ─────────────────────────────────────────────────────────────────

