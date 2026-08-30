using FinalProject_SeventhSem.Domain.Common;
using FinalProject_SeventhSem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Domain.Entities;

public class Organization : BaseEntity
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? Description { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LogoUrl { get; set; }
    public OrganizationStatus Status { get; set; } = OrganizationStatus.Pending;

    public User User { get; set; } = null!;
    public ICollection<Vacancy> Vacancies { get; set; } = new List<Vacancy>();
}
