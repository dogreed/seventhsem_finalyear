using FinalProject_SeventhSem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure.Persistence.Configurations;

public class SkillAliasConfiguration : IEntityTypeConfiguration<SkillAlias>
{
    public void Configure(EntityTypeBuilder<SkillAlias> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Alias).IsRequired().HasMaxLength(100);

        builder.HasIndex(a => a.Alias).IsUnique();
    }
}
