using FinalProject_SeventhSem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Models.Applications;

public record ApplyToVacancyRequest(
    int VacancyId
);

public record UpdateApplicationStatusRequest(
    ApplicationStatus NewStatus
);


public record ApplicationResponse(
    int ApplicationId,
    int VacancyId,
    string VacancyTitle,
    int StudentId,
    string StudentName,
    string Status,
    DateTime AppliedAt,
    DateTime? StatusUpdatedAt,
    ApplicationSnapshotDto? MatchSnapshot
);

public record OrganizationApplicationResponse(
    int ApplicationId,
    int VacancyId,
    string VacancyTitle,
    int StudentId,
    string StudentName,
    string Status,
    DateTime AppliedAt,
    string? ResumeUrl,
    int TestCount
);

public record ApplicationSnapshotDto(
    double RequirementFit,
    double OptionalFit,
    double EducationBonus,
    IReadOnlyList<string> MissingSkills,
    DateTime ComputedAt
);

