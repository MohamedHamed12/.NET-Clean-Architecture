using Infrastructure.Identity.Context;
using Infrastructure.Identity.Seeds;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Web.Api;
using Serilog;

namespace FunctionalTest.WebApi.Factory
{
    public class AppFactory : WebApplicationFactory<Startup>
    {
        private SqliteConnection _identityConnection;
        private SqliteConnection _appConnection;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // *** Aggressively remove ANY service related to EntityFrameworkCore ***
                // This targets all DbContexts, DbContextOptions, and potentially internal EF Core services
                // that might be implicitly tied to a specific provider (like SQL Server).
                var efCoreDescriptorsToRemove = services.Where(d =>
                    d.ServiceType.FullName != null &&
                    (d.ServiceType.FullName.Contains("Microsoft.EntityFrameworkCore") || // Catches all EF Core services
                     d.ServiceType.FullName.Contains(typeof(IdentityContext).FullName) || // Specific context type
                     d.ServiceType.FullName.Contains(typeof(AppDbContext).FullName))     // Specific context type
                ).ToList();

                // Iterate backwards to safely remove elements as you modify the collection
                for (int i = services.Count - 1; i >= 0; i--)
                {
                    var descriptor = services[i];
                    if (efCoreDescriptorsToRemove.Contains(descriptor))
                    {
                        services.RemoveAt(i);
                    }
                }

                // *** Remove existing Authentication and Identity related services ***
                // This prevents "Scheme already exists" errors by clearing prior registrations.
                var authAndIdentityDescriptorsToRemove = services.Where(d =>
                    d.ServiceType.FullName != null &&
                    (d.ServiceType.FullName.Contains("Microsoft.AspNetCore.Authentication") ||
                     d.ServiceType.FullName.Contains("Microsoft.AspNetCore.Identity"))
                ).ToList();

                for (int i = services.Count - 1; i >= 0; i--)
                {
                    var descriptor = services[i];
                    if (authAndIdentityDescriptorsToRemove.Contains(descriptor))
                    {
                        services.RemoveAt(i);
                    }
                }

                // Now, configure your DbContexts to use SQLite in-memory databases FIRST
                // This ensures AddEntityFrameworkStores has the correct context to reference.
                ConfigureSqliteDbContext<IdentityContext>(services, out _identityConnection);
                ConfigureSqliteDbContext<AppDbContext>(services, out _appConnection);

                // IMPORTANT: Re-register your Identity services AFTER setting up DbContexts.
                // The configuration below mirrors what's typically in your Startup.cs for Identity setup.
                services.AddIdentity<Core.Domain.Identity.Entities.ApplicationUser, Core.Domain.Identity.Entities.ApplicationRole>(options => {
                    options.Password.RequireDigit = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequiredLength = 4;
                })
                .AddEntityFrameworkStores<IdentityContext>() // This will now use the SQLite IdentityContext you configure above
                .AddClaimsPrincipalFactory<Infrastructure.Identity.Identity.CustomUserClaimsPrincipalFactory>()
                .AddDefaultTokenProviders();

                // Ensure the MockSchemeProvider is used for authentication in tests
                services.AddSingleton<IAuthenticationSchemeProvider, MockSchemeProvider>();
            });
        }

        private void ConfigureSqliteDbContext<TContext>(IServiceCollection services, out SqliteConnection connection) where TContext : DbContext
        {
            var sqliteConnection = new SqliteConnection("DataSource=:memory:");
            sqliteConnection.Open();
            connection = sqliteConnection;

            services.AddDbContext<TContext>(options =>
            {
                options.UseSqlite(sqliteConnection);
            });

            // Ensure the database is created and migrated within the test scope
            using var scope = services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();
            try
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);
            // Ensure seeding happens after our database setup
            IdentityMigrationManager.SeedDatabaseAsync(host).GetAwaiter().GetResult();
            return host;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _identityConnection?.Close();
            _identityConnection?.Dispose();
            _appConnection?.Close();
            _appConnection?.Dispose();
        }
    }
}