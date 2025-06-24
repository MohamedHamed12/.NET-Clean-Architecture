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
using System;
using System.Linq;
using Web.Mvc;

namespace FunctionalTest.Mvc
{
    public class AppFactory : WebApplicationFactory<Startup>
    {
        private SqliteConnection _identityConnection;
        private SqliteConnection _appConnection;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // *** ULTIMATE FIX: Aggressively remove ANY service related to EntityFrameworkCore
                // or your specific DbContexts before re-configuring them for SQLite. ***
                var descriptorsToRemove = services.Where(d =>
                    d.ServiceType.FullName != null &&
                    (d.ServiceType.FullName.Contains("Microsoft.EntityFrameworkCore") || // Catches all EF Core services
                     d.ServiceType.FullName.Contains(typeof(IdentityContext).FullName) || // Specific context type
                     d.ServiceType.FullName.Contains(typeof(AppDbContext).FullName))     // Specific context type
                ).ToList();

                // Iterate backwards to safely remove elements as you modify the collection
                for (int i = services.Count - 1; i >= 0; i--)
                {
                    var descriptor = services[i];
                    if (descriptorsToRemove.Contains(descriptor))
                    {
                        services.RemoveAt(i);
                    }
                }

                // *** FIX FOR "Scheme already exists: Identity.Application" ***
                // Remove existing Authentication and Identity related services
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
                // This ensures Identity.AddEntityFrameworkStores has the correct context to reference.
                ConfigureSqliteDbContext<IdentityContext>(services, out _identityConnection);
                ConfigureSqliteDbContext<AppDbContext>(services, out _appConnection);

                // IMPORTANT: Re-register your Identity services AFTER removing the old ones and setting up DbContexts.
                // This ensures Identity uses your new SQLite context setup.
                // The configuration below mirrors what's in your Startup.cs for Identity setup.
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

            using var scope = services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();
            try
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting up {typeof(TContext).Name}: {ex.Message}");
                throw;
            }
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);
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