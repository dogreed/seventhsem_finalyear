using FinalProject_SeventhSem.Domain.Common;
using FinalProject_SeventhSem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Domain.Entities;

public class Application : BaseEntity
{
    public int StudentId { get; set; }
    public int VacancyId { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StatusUpdatedAt { get; set; }

    public Student Student { get; set; } = null!;
    public Vacancy Vacancy { get; set; } = null!;
    public ApplicationMatchSnapshot? MatchSnapshot { get; set; }
}
