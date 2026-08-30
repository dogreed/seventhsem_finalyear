using FinalProject_SeventhSem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Domain.Entities;

public class ResourceSkillMapping : BaseEntity
{
    public int ResourceId { get; set; }
    public int SkillId { get; set; }

    public Resource Resource { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}