using FinalProject_SeventhSem.Application.Common;
using FinalProject_SeventhSem.Application.Interfaces;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Students.Commands.UploadProfilePhoto;

public record UploadProfilePhotoCommand(
    int UserId,
    Stream ImageStream,
    string FileName
) : IRequest<string>; // returns new PhotoUrl

// ── Validator ─────────────────────────────────────────────────────────────────

public class UploadProfilePhotoCommandValidator : AbstractValidator<UploadProfilePhotoCommand>
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public UploadProfilePhotoCommandValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .Must(f => AllowedExtensions.Any(ext =>
                f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            .WithMessage("Only JPG, PNG, or WEBP image files are accepted.");

        RuleFor(x => x.ImageStream)
            .NotNull()
            .Must(s => s.Length > 0)
            .WithMessage("Uploaded image is empty.")
            .Must(s => s.Length <= 5 * 1024 * 1024)
            .WithMessage("Image must not exceed 5 MB.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public class UploadProfilePhotoCommandHandler : IRequestHandler<UploadProfilePhotoCommand, string>
{
    private readonly IRepository<Student> _studentRepo;
    private readonly IUnitOfWork _uow;
    private readonly IFileStorageService _storage;

    public UploadProfilePhotoCommandHandler(
        IRepository<Student> studentRepo,
        IUnitOfWork uow,
        IFileStorageService storage)
    {
        _studentRepo = studentRepo;
        _uow = uow;
        _storage = storage;
    }

    public async Task<string> Handle(
        UploadProfilePhotoCommand request, CancellationToken cancellationToken)
    {
        var student = await StudentResolver.ResolveAsync(request.UserId, _studentRepo, cancellationToken);

        // Delete old photo if exists
        if (!string.IsNullOrWhiteSpace(student.PhotoUrl))
            await _storage.DeleteAsync(student.PhotoUrl, cancellationToken);

        var url = await _storage.SaveAsync(
            request.ImageStream, request.FileName, "photos", cancellationToken);

        student.PhotoUrl = url;
        student.UpdatedAt = DateTime.UtcNow;
        _studentRepo.Update(student);
        await _uow.SaveChangesAsync(cancellationToken);

        return url;
    }
}

