using FinalProject_SeventhSem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure.Persistence.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Name).IsRequired().HasMaxLength(150);
        builder.Property(o => o.WebsiteUrl).HasMaxLength(300);
        builder.Property(o => o.LogoUrl).HasMaxLength(500);

        builder.HasMany(o => o.Vacancies)
            .WithOne(v => v.Organization)
            .HasForeignKey(v => v.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
