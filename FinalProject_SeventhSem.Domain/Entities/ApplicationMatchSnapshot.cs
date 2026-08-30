using FinalProject_SeventhSem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Domain.Entities;

public class ApplicationMatchSnapshot : BaseEntity
{
    public int ApplicationId { get; set; }

    /// <summary>Percentage of required skills matched. Output of Algorithm 4.</summary>
    public double RequirementFit { get; set; }

    /// <summary>Percentage of optional skills matched. Output of Algorithm 5.</summary>
    public double OptionalFit { get; set; }

    /// <summary>+5 if student's optional education matched; 0 otherwise.</summary>
    public double EducationBonus { get; set; }

    public string MissingSkillIdsJson { get; set; } = "[]";

    public DateTime ComputedAt { get; set; } = DateTime.UtcNow;

    public Application Application { get; set; } = null!;
}
