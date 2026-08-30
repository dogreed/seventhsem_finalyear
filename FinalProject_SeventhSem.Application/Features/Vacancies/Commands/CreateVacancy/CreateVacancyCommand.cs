using FinalProject_SeventhSem.Application.Models.Vacancies;
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

namespace FinalProject_SeventhSem.Application.Features.Vacancies.Commands.CreateVacancy;

public record CreateVacancyCommand(
    int OrganizationId,
    string Title,
    string Description,
    DateTime ApplicationDeadline,
    EducationLevel? RequiredEducationLevel,
    string? RequiredFieldOfStudy,
    IReadOnlyList<int> RequiredSkillIds,
    IReadOnlyList<int>? OptionalSkillIds
) : IRequest<VacancyResponse>;

// ── Validator ─────────────────────────────────────────────────────────────────

