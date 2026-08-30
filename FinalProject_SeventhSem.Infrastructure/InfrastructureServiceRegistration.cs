using FinalProject_SeventhSem.Application.Interfaces;
using FinalProject_SeventhSem.Domain.Interfaces;
using FinalProject_SeventhSem.Infrastructure.Engines;
using FinalProject_SeventhSem.Infrastructure.Persistence;
using FinalProject_SeventhSem.Infrastructure.Persistence.Interceptors;
using FinalProject_SeventhSem.Infrastructure.Persistence.Repositories;
using FinalProject_SeventhSem.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<AuditInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly(
                    typeof(InfrastructureServiceRegistration).Assembly.FullName));

            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IJwtService, JwtService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        var uploadsPath = configuration["FileStorage:BasePath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        services.AddScoped<IFileStorageService>(_ => new FileStorageService(uploadsPath));

        services.AddScoped<IResumeParsingService, ResumeParsingEngine>();
        services.AddScoped<IMatchingService, MatchingEngine>();
        services.AddScoped<IScoringService, ScoringEngine>();

        services.AddScoped<FinalProject_SeventhSem.Infrastructure.Seeders.DatabaseSeeder>();

        return services;
    }
}
