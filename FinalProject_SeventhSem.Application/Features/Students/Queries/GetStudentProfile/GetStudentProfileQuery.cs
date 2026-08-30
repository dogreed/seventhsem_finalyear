using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Application.Models.Students;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Students.Queries.GetStudentProfile;

/// <summary>UserId comes from the JWT sub claim (not the Student table PK).</summary>
public record GetStudentProfileQuery(int UserId) : IRequest<StudentProfileResponse>;


// ── Handler ───────────────────────────────────────────────────────────────────

