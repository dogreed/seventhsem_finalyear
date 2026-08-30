using FinalProject_SeventhSem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Domain.Entities;

public class Skill : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<SkillAlias> Aliases { get; set; } = new List<SkillAlias>();
    public ICollection<StudentSkill> StudentSkills { get; set; } = new List<StudentSkill>();
    public ICollection<VacancySkill> VacancySkills { get; set; } = new List<VacancySkill>();
    public ICollection<ResourceSkillMapping> ResourceSkillMappings { get; set; } = new List<ResourceSkillMapping>();
}
