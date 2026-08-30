using FinalProject_SeventhSem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Models.Students;

public record UpdateStudentProfileRequest(
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
);


public record StudentProfileResponse(
    int StudentId,
    int UserId,
    string FullName,
    string Email,
    string? PhotoUrl,
    string? PhoneNumber,
    string? Bio,
    string? Nationality,
    string? Location,
    string? EducationLevel,
    string? FieldOfStudy,
    string? GitHubUrl,
    string? PortfolioUrl,
    string? LinkedInUrl,
    string? ResumeUrl,
    IReadOnlyList<SkillSummary> ConfirmedSkills
    
);

public record SkillSummary(
    int SkillId,
    string Name
);


/// <summary>
/// Profile completeness breakdown. Computed on the fly (Option A — no DB column).
/// TotalScore is a sum of individual field points (max 100).
/// </summary>
public record ProfileCompletenessResponse(
    int TotalScore,
    bool HasFullName,
    bool HasPhoto,
    bool HasPhoneNumber,
    bool HasEducation,
    int SkillPoints,        // 0 | 5 | 10 | 20 depending on confirmed skill count
    bool HasResume,
    bool HasGitHub,
    bool HasPortfolio,
    bool HasLinkedIn,
    bool HasBio,
    bool HasNationality,
    int TotalApplicationApplied,
    int TotalTestsAttempted
);
