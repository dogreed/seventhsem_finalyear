using FinalProject_SeventhSem.Application.Common;
using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Features.Students.Commands.UpdateStudentProfile;

public class UpdateStudentProfileCommandHandler : IRequestHandler<UpdateStudentProfileCommand>
{
    private readonly IRepository<Student> _studentRepo;
    private readonly IUnitOfWork _uow;

    public UpdateStudentProfileCommandHandler(IRepository<Student> studentRepo, IUnitOfWork uow)
    {
        _studentRepo = studentRepo;
        _uow = uow;
    }

    public async Task Handle(UpdateStudentProfileCommand request, CancellationToken cancellationToken)
    {
        var student = await StudentResolver.ResolveAsync(request.UserId, _studentRepo, cancellationToken);

        student.FullName = request.FullName;
        student.PhoneNumber = request.PhoneNumber;
        student.Bio = request.Bio;
        student.Nationality = request.Nationality;
        student.Location = request.Location;
        student.EducationLevel = request.EducationLevel;
        student.FieldOfStudy = request.FieldOfStudy;
        student.GitHubUrl = request.GitHubUrl;
        student.PortfolioUrl = request.PortfolioUrl;
        student.LinkedInUrl = request.LinkedInUrl;
        student.UpdatedAt = DateTime.UtcNow;

        _studentRepo.Update(student);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}


