using FinalProject_SeventhSem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Domain.Entities;

public class VacancySkill : BaseEntity
{
    public int VacancyId { get; set; }
    public int SkillId { get; set; }
    public bool IsRequired { get; set; }

    public Vacancy Vacancy { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}

