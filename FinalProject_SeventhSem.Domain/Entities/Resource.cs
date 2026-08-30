using FinalProject_SeventhSem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Domain.Entities;

public class Resource : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Url { get; set; } = string.Empty;

    public string? ResourceType { get; set; }

    public ICollection<ResourceSkillMapping> SkillMappings { get; set; } = new List<ResourceSkillMapping>();
    public ICollection<ResourceRating> Ratings { get; set; } = new List<ResourceRating>();
}