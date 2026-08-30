using FinalProject_SeventhSem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure.Persistence.Configurations;

public class StudentSeenQuestionConfiguration : IEntityTypeConfiguration<StudentSeenQuestion>
{
    public void Configure(EntityTypeBuilder<StudentSeenQuestion> builder)
    {
        builder.HasKey(sq => sq.Id);

        builder.HasIndex(sq => new { sq.StudentId, sq.QuestionId }).IsUnique();

        builder.HasOne(sq => sq.Question)
            .WithMany(q => q.SeenByStudents)
            .HasForeignKey(sq => sq.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
