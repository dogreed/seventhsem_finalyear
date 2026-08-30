using FinalProject_SeventhSem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure.Persistence.Configurations;

public class VacancyConfiguration : IEntityTypeConfiguration<Vacancy>
{
    public void Configure(EntityTypeBuilder<Vacancy> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Title).IsRequired().HasMaxLength(200);
        builder.Property(v => v.Description).IsRequired().HasMaxLength(5000);
        builder.Property(v => v.RequiredEducationLevel).HasConversion<string>().HasMaxLength(20);
        builder.Property(v => v.RequiredFieldOfStudy).HasMaxLength(150);
        builder.Property(v => v.ApplicationDeadline)
            .IsRequired()
            .HasColumnType("datetime2");
        builder.HasMany(v => v.VacancySkills)
            .WithOne(vs => vs.Vacancy)
            .HasForeignKey(vs => vs.VacancyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.Applications)
            .WithOne(a => a.Vacancy)
            .HasForeignKey(a => a.VacancyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


