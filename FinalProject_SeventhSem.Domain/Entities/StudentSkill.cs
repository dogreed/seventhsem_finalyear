using FinalProject_SeventhSem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Domain.Entities;

public class StudentSkill : BaseEntity
{
    public int StudentId { get; set; }
    public int SkillId { get; set; }

    public DateTime ConfirmedAt { get; set; } = DateTime.UtcNow;

    public Student Student { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
