using FinalProject_SeventhSem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure.Persistence.Configurations;

public class VacancySkillConfiguration : IEntityTypeConfiguration<VacancySkill>
{
    public void Configure(EntityTypeBuilder<VacancySkill> builder)
    {
        builder.HasKey(vs => vs.Id);
        builder.HasIndex(vs => new { vs.VacancyId, vs.SkillId }).IsUnique();

        builder.HasOne(vs => vs.Skill)
            .WithMany(s => s.VacancySkills)
            .HasForeignKey(vs => vs.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
