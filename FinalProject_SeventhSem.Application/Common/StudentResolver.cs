using FinalProject_SeventhSem.Application.Exceptions;
using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application.Common;


public static class StudentResolver
{
    public static async Task<Student> ResolveAsync(
        int userId,
        IRepository<Student> studentRepo,
        CancellationToken ct = default)
    {
        var students = await studentRepo.GetAllAsync(ct);
        var student = students.FirstOrDefault(s => s.UserId == userId);
        return student ?? throw new NotFoundException(
            $"No student profile found for UserId {userId}. " +
            "Ensure the student profile exists.");
    }
}
