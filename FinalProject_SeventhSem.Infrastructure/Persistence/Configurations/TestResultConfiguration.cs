using FinalProject_SeventhSem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure.Persistence.Configurations;

public class TestResultConfiguration : IEntityTypeConfiguration<TestResult>
{
    public void Configure(EntityTypeBuilder<TestResult> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Score).HasColumnType("float");
        builder.Property(r => r.ChapterScoresJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(r => r.WeakChapterIdsJson).IsRequired().HasColumnType("nvarchar(max)");

        builder.HasIndex(r => new { r.StudentId, r.IsLatest })
            .HasFilter("[IsLatest] = 1");
    }
}