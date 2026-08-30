using FinalProject_SeventhSem.Domain.Common;
using FinalProject_SeventhSem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Domain.Entities;

public class Student : BaseEntity
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
    public string? Nationality { get; set; }
    public string? Location { get; set; }

    public EducationLevel? EducationLevel { get; set; }
    public string? FieldOfStudy { get; set; }

    public string? GitHubUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? LinkedInUrl { get; set; }

    public string? ResumeUrl { get; set; }

    public User User { get; set; } = null!;
    public ICollection<StudentSkill> StudentSkills { get; set; } = new List<StudentSkill>();
    public ICollection<Application> Applications { get; set; } = new List<Application>();
    public ICollection<Test> Tests { get; set; } = new List<Test>();
    public ICollection<StudentSeenQuestion> SeenQuestions { get; set; } = new List<StudentSeenQuestion>();
}

