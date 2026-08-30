using FinalProject_SeventhSem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure.Persistence.Configurations;

public class StackConfiguration : IEntityTypeConfiguration<Stack>
{
    public void Configure(EntityTypeBuilder<Stack> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(s => s.Name).IsUnique();

        builder.HasMany(s => s.Chapters)
            .WithOne(c => c.Stack)
            .HasForeignKey(c => c.StackId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ChapterConfiguration : IEntityTypeConfiguration<Chapter>
{
    public void Configure(EntityTypeBuilder<Chapter> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(150);

        builder.HasMany(c => c.Questions)
            .WithOne(q => q.Chapter)
            .HasForeignKey(q => q.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Text).IsRequired().HasMaxLength(1000);
        builder.Property(q => q.OptionA).IsRequired().HasMaxLength(500);
        builder.Property(q => q.OptionB).IsRequired().HasMaxLength(500);
        builder.Property(q => q.OptionC).IsRequired().HasMaxLength(500);
        builder.Property(q => q.OptionD).IsRequired().HasMaxLength(500);
        builder.Property(q => q.CorrectOption).IsRequired().HasMaxLength(1);
    }
}

public class TestConfiguration : IEntityTypeConfiguration<Test>
{
    public void Configure(EntityTypeBuilder<Test> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Status).HasConversion<string>().IsRequired();

        builder.HasMany(t => t.Answers)
            .WithOne(a => a.Test)
            .HasForeignKey(a => a.TestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Result)
            .WithOne(r => r.Test)
            .HasForeignKey<TestResult>(r => r.TestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TestAnswerConfiguration : IEntityTypeConfiguration<TestAnswer>
{
    public void Configure(EntityTypeBuilder<TestAnswer> builder)
    {
        builder.HasKey(a => a.Id);

        builder.HasIndex(a => new { a.TestId, a.QuestionId }).IsUnique();

        builder.Property(a => a.SelectedOption).IsRequired().HasMaxLength(1);

        builder.HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}





