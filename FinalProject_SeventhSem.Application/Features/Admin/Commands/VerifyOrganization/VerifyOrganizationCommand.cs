using FinalProject_SeventhSem.Domain.Enums;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Admin.Commands.VerifyOrganization;

public record VerifyOrganizationCommand(int OrganizationId, OrganizationStatus Status, string? Reason = null) : IRequest<VerifyOrganizationResponse>;

public class VerifyOrganizationCommandValidator : AbstractValidator<VerifyOrganizationCommand>
{
    public VerifyOrganizationCommandValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);

        RuleFor(x => x.Status)
            .Must(s => s != OrganizationStatus.Pending)
            .WithMessage("Cannot manually set status to Pending.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .When(x => x.Status == OrganizationStatus.Rejected || x.Status == OrganizationStatus.Suspended)
            .WithMessage("A reason is required when rejecting or suspending an organization.");
    }
}

public record VerifyOrganizationResponse(
    int OrganizationId,
    string OrganizationName,
    string Email,
    OrganizationStatus Status,
    bool IsActive,
    DateTime UpdatedAt
);