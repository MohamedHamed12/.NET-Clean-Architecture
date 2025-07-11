// File: Factory/AppFactory.cs
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
using Serilog;
using Web.Api;

namespace FunctionalTest.WebApi.Factory
{
    public class AppFactory : WebApplicationFactory<Startup>
    {
        private SqliteConnection _identityConnection = null!;
        private SqliteConnection _appConnection = null!;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var efCoreDescriptorsToRemove = services.Where(d =>
                    d.ServiceType.FullName != null &&
                    (d.ServiceType.FullName.Contains("Microsoft.EntityFrameworkCore") ||
                     d.ServiceType.FullName.Contains(typeof(IdentityContext).FullName) ||
                     d.ServiceType.FullName.Contains(typeof(AppDbContext).FullName))
                ).ToList();

                foreach (var descriptor in efCoreDescriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                var authAndIdentityDescriptorsToRemove = services.Where(d =>
                    d.ServiceType.FullName != null &&
                    (d.ServiceType.FullName.Contains("Microsoft.AspNetCore.Authentication") ||
                     d.ServiceType.FullName.Contains("Microsoft.AspNetCore.Identity"))
                ).ToList();

                foreach (var descriptor in authAndIdentityDescriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                _identityConnection = CreateInMemoryDatabase<IdentityContext>(services);
                _appConnection = CreateInMemoryDatabase<AppDbContext>(services);

                services.AddIdentity<Core.Domain.Identity.Entities.ApplicationUser, Core.Domain.Identity.Entities.ApplicationRole>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequiredLength = 4;
                })
                .AddEntityFrameworkStores<IdentityContext>()
                .AddClaimsPrincipalFactory<Infrastructure.Identity.Identity.CustomUserClaimsPrincipalFactory>()
                .AddDefaultTokenProviders();

                services.AddSingleton<IAuthenticationSchemeProvider, MockSchemeProvider>();
            });
        }

        private SqliteConnection CreateInMemoryDatabase<TContext>(IServiceCollection services) where TContext : DbContext
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            services.AddDbContext<TContext>(options => options.UseSqlite(connection));

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

            return connection;
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
