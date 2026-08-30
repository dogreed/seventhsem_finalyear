using FinalProject_SeventhSem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure.Persistence.Configurations;

public class ApplicationConfiguration : IEntityTypeConfiguration<FinalProject_SeventhSem.Domain.Entities.Application>
{
    public void Configure(EntityTypeBuilder<FinalProject_SeventhSem.Domain.Entities.Application> builder)
    {
        builder.HasKey(a => a.Id);

        builder.HasIndex(a => new { a.StudentId, a.VacancyId }).IsUnique();

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.HasOne(a => a.MatchSnapshot)
            .WithOne(s => s.Application)
            .HasForeignKey<ApplicationMatchSnapshot>(s => s.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
