using AspNetCoreRateLimit;
using FinalProject_SeventhSem.Application;
using FinalProject_SeventhSem.Application.Common.Settings;
using FinalProject_SeventhSem.Infrastructure;
using FinalProject_SeventhSem.Infrastructure.Persistence;
using FinalProject_SeventhSem.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Text;

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
	.Enrich.FromLogContext()
	.WriteTo.Console()
	.CreateBootstrapLogger();

try
{
	var builder = WebApplication.CreateBuilder(args);

	// -------------------------------------------------------
	// Serilog
	// -------------------------------------------------------
	builder.Host.UseSerilog((ctx, services, config) => config
		.ReadFrom.Configuration(ctx.Configuration)
		.ReadFrom.Services(services)
		.Enrich.FromLogContext()
		.WriteTo.Console()
		.WriteTo.File(
			"logs/internhub-.log",
			rollingInterval: RollingInterval.Day,
			retainedFileCountLimit: 14));

	// -------------------------------------------------------
	// Application / Infrastructure
	// -------------------------------------------------------
	builder.Services.AddApplicationServices(builder.Configuration);
	builder.Services.AddInfrastructureServices(builder.Configuration);

	// -------------------------------------------------------
	// JWT Settings
	// -------------------------------------------------------
	var jwtSection = builder.Configuration.GetSection(JwtSettings.SectionName);
	var jwtSettings = jwtSection.Get<JwtSettings>()
		?? throw new InvalidOperationException(
			"JwtSettings configuration is missing.");

	builder.Services
		.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme =
				JwtBearerDefaults.AuthenticationScheme;

			options.DefaultChallengeScheme =
				JwtBearerDefaults.AuthenticationScheme;
		})
		.AddJwtBearer(options =>
		{
			options.TokenValidationParameters =
				new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,

					IssuerSigningKey =
						new SymmetricSecurityKey(
							Encoding.UTF8.GetBytes(
								jwtSettings.SecretKey)),

					ValidateIssuer = true,
					ValidIssuer = jwtSettings.Issuer,

					ValidateAudience = true,
					ValidAudience = jwtSettings.Audience,

					ValidateLifetime = true,

					ClockSkew = TimeSpan.Zero
				};
		});

	builder.Services.AddAuthorization();

	// -------------------------------------------------------
	// Rate Limiting
	// -------------------------------------------------------
	builder.Services.AddMemoryCache();

	builder.Services.Configure<IpRateLimitOptions>(
		builder.Configuration.GetSection("IpRateLimiting"));

	builder.Services.AddSingleton<IIpPolicyStore,
		MemoryCacheIpPolicyStore>();

	builder.Services.AddSingleton<IRateLimitCounterStore,
		MemoryCacheRateLimitCounterStore>();

	builder.Services.AddSingleton<IRateLimitConfiguration,
		RateLimitConfiguration>();

	builder.Services.AddSingleton<IProcessingStrategy,
		AsyncKeyLockProcessingStrategy>();

	builder.Services.AddInMemoryRateLimiting();

	// -------------------------------------------------------
	// Controllers
	// -------------------------------------------------------
	builder.Services.AddControllers();

	builder.Services.AddEndpointsApiExplorer();

	// -------------------------------------------------------
	// Swagger
	// -------------------------------------------------------
	builder.Services.AddSwaggerGen(c =>
	{
		c.SwaggerDoc("v1", new OpenApiInfo
		{
			Title = "InternHub API",
			Version = "v1",
			Description =
				"Deterministic internship matching platform — no ML, no AI."
		});

		c.AddSecurityDefinition("Bearer",
			new OpenApiSecurityScheme
			{
				Name = "Authorization",
				Type = SecuritySchemeType.ApiKey,
				Scheme = "Bearer",
				BearerFormat = "JWT",
				In = ParameterLocation.Header,
				Description = "Enter: Bearer {token}"
			});

		c.AddSecurityRequirement(
			new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = "Bearer"
						}
					},
					Array.Empty<string>()
				}
			});
	});

	// -------------------------------------------------------
	// CORS
	// -------------------------------------------------------
	builder.Services.AddCors(options =>
		options.AddDefaultPolicy(policy =>
			policy
				.AllowAnyOrigin()
				.AllowAnyMethod()
				.AllowAnyHeader()));

	// -------------------------------------------------------
	// Build application
	// -------------------------------------------------------
	var app = builder.Build();

	app.Logger.LogInformation(
		"App built successfully, starting pipeline...");

	// -------------------------------------------------------
	// IMPORTANT:
	// Do NOT run Database.Migrate() here.
	//
	// Use EF Core commands instead:
	//
	// Add-Migration MigrationName
	// Update-Database
	//
	// AppDbContextFactory handles EF design-time creation.
	// -------------------------------------------------------


	using (var scope = app.Services.CreateScope())
	{
		var db = scope.ServiceProvider
			.GetRequiredService<AppDbContext>();

		db.Database.Migrate();

		var seeder = scope.ServiceProvider
			.GetRequiredService<
				FinalProject_SeventhSem.Infrastructure.Seeders.DatabaseSeeder>();

		await seeder.SeedAsync();
	}


	// -------------------------------------------------------
	// Middleware
	// -------------------------------------------------------
	app.UseCors();
	app.UseIpRateLimiting();

	if (app.Environment.IsDevelopment())
	{
		app.UseSwagger();

		app.UseSwaggerUI(c =>
			c.SwaggerEndpoint(
				"/swagger/v1/swagger.json",
				"InternHub v1"));
	}

	app.UseSerilogRequestLogging();

	app.UseMiddleware<ExceptionMiddleware>();

	app.UseHttpsRedirection();

	app.UseStaticFiles();



	app.UseAuthentication();

	app.UseAuthorization();

	app.MapControllers();

	// -------------------------------------------------------
	// Run
	// -------------------------------------------------------
	await app.RunAsync();
}
catch (Microsoft.Extensions.Hosting.HostAbortedException)
{
	// EF Core may intentionally abort the host during
	// design-time operations such as Add-Migration or
	// Update-Database.
}
catch (Exception ex)
{
	Log.Fatal(
		ex,
		"Application terminated unexpectedly");
}
finally
{
	Log.CloseAndFlush();
}
