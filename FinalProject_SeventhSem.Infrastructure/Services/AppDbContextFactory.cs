using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinalProject_SeventhSem.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
	public AppDbContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

		optionsBuilder.UseSqlServer(
			"Server=.\\SQLEXPRESS;Database=TalentMap;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False",
			sqlOptions =>
				sqlOptions.MigrationsAssembly(
					typeof(AppDbContext).Assembly.FullName));

		return new AppDbContext(optionsBuilder.Options);
	}
}