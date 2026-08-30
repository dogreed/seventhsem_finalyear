using FinalProject_SeventhSem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure.Persistence.Configurations;

public class ResourceSkillMappingConfiguration : IEntityTypeConfiguration<ResourceSkillMapping>
{
    public void Configure(EntityTypeBuilder<ResourceSkillMapping> builder)
    {
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => new { m.ResourceId, m.SkillId }).IsUnique();

        builder.HasOne(m => m.Skill)
            .WithMany(s => s.ResourceSkillMappings)
            .HasForeignKey(m => m.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}