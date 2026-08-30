using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Interfaces;
using FinalProject_SeventhSem.Application.Models.Resume;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Resume.Commands.UploadResume;

public record UploadResumeCommand(
    int UserId,
    Stream PdfStream,
    string FileName
) : IRequest<ResumeParseResponse>;

// ── Validator ─────────────────────────────────────────────────────────────────


