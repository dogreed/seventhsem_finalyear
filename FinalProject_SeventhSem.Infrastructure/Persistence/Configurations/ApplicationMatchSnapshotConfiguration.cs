using FinalProject_SeventhSem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure.Persistence.Configurations;

public class ApplicationMatchSnapshotConfiguration
    : IEntityTypeConfiguration<ApplicationMatchSnapshot>
{
    public void Configure(EntityTypeBuilder<ApplicationMatchSnapshot> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.MissingSkillIdsJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(s => s.RequirementFit).HasColumnType("float");
        builder.Property(s => s.OptionalFit).HasColumnType("float");
        builder.Property(s => s.EducationBonus).HasColumnType("float");
    }
}

