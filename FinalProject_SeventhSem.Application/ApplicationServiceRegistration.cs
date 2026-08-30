using FinalProject_SeventhSem.Application.Behaviors;
using FinalProject_SeventhSem.Application.Common.Settings;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject_SeventhSem.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceRegistration).Assembly));

        services.AddValidatorsFromAssembly(
            typeof(ApplicationServiceRegistration).Assembly,
            includeInternalTypes: true);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));

        services.Configure<ThresholdSettings>(
            configuration.GetSection(ThresholdSettings.SectionName));

        services.Configure<TestSettings>(
            configuration.GetSection(TestSettings.SectionName));

        services.Configure<ResumeParsingSettings>(
            configuration.GetSection(ResumeParsingSettings.SectionName));

        services.Configure<AdminSeedSettings>(
            configuration.GetSection(AdminSeedSettings.SectionName));

        return services;
    }
}