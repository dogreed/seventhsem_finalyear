using FinalProject_SeventhSem.Domain.Entities;
using FinalProject_SeventhSem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace FinalProject_SeventhSem.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<SkillAlias> SkillAliases => Set<SkillAlias>();
    public DbSet<StudentSkill> StudentSkills => Set<StudentSkill>();

    public DbSet<Vacancy> Vacancies => Set<Vacancy>();
    public DbSet<VacancySkill> VacancySkills => Set<VacancySkill>();
    public DbSet<FinalProject_SeventhSem.Domain.Entities.Application> Applications => Set<FinalProject_SeventhSem.Domain.Entities.Application>();
    public DbSet<ApplicationMatchSnapshot> ApplicationMatchSnapshots => Set<ApplicationMatchSnapshot>();

    public DbSet<Stack> Stacks => Set<Stack>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Test> Tests => Set<Test>();
    public DbSet<TestAnswer> TestAnswers => Set<TestAnswer>();
    public DbSet<TestResult> TestResults => Set<TestResult>();
    public DbSet<StudentSeenQuestion> StudentSeenQuestions => Set<StudentSeenQuestion>();

    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<ResourceSkillMapping> ResourceSkillMappings => Set<ResourceSkillMapping>();
    public DbSet<ResourceRating> ResourceRatings => Set<ResourceRating>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

