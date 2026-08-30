using FinalProject_SeventhSem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure.Persistence.Configurations;

public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Title).IsRequired().HasMaxLength(300);
        builder.Property(r => r.Url).IsRequired().HasMaxLength(500);
        builder.Property(r => r.ResourceType).HasMaxLength(50);
        builder.Property(r => r.Description).HasMaxLength(1000);

        builder.HasMany(r => r.SkillMappings)
            .WithOne(m => m.Resource)
            .HasForeignKey(m => m.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Ratings)
            .WithOne(rt => rt.Resource)
            .HasForeignKey(rt => rt.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}



public class ResourceRatingConfiguration : IEntityTypeConfiguration<ResourceRating>
{
    public void Configure(EntityTypeBuilder<ResourceRating> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => new { r.ResourceId, r.StudentId }).IsUnique();

        builder.Property(r => r.Rating).IsRequired();
        builder.Property(r => r.Comment).HasMaxLength(500);

        builder.HasOne(r => r.Student)
            .WithMany()
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}





