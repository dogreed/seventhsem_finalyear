using FinalProject_SeventhSem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FullName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.PhotoUrl).HasMaxLength(500);
        builder.Property(s => s.PhoneNumber).HasMaxLength(20);
        builder.Property(s => s.Bio).HasMaxLength(1000);
        builder.Property(s => s.Nationality).HasMaxLength(100);
        builder.Property(s => s.Location).HasMaxLength(200);
        builder.Property(s => s.FieldOfStudy).HasMaxLength(150);
        builder.Property(s => s.GitHubUrl).HasMaxLength(300);
        builder.Property(s => s.PortfolioUrl).HasMaxLength(300);
        builder.Property(s => s.LinkedInUrl).HasMaxLength(300);
        builder.Property(s => s.ResumeUrl).HasMaxLength(500);

        builder.Property(s => s.EducationLevel)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasMany(s => s.StudentSkills)
            .WithOne(ss => ss.Student)
            .HasForeignKey(ss => ss.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Applications)
            .WithOne(a => a.Student)
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Tests)
            .WithOne(t => t.Student)
            .HasForeignKey(t => t.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.SeenQuestions)
            .WithOne(sq => sq.Student)
            .HasForeignKey(sq => sq.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
