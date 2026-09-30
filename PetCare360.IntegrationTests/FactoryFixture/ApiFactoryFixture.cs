using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PetCare360.Domain.Interfaces;
using PetCare360.Infrastructure.Data;
using PetCare360.IntegrationTests.Fakes;

namespace PetCare360.IntegrationTests.FactoryFixture
{
    public class ApiFactoryFixture : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:OracleConnection"] =
                        "User Id=teste;Password=teste;Data Source=localhost:1521/XEPDB1;"
                });
            });

            builder.ConfigureServices(services =>
            {
                var descritores = services
                    .Where(d => d.ServiceType.FullName != null &&
                                (d.ServiceType.FullName.Contains("DbContextOptions") ||
                                 d.ServiceType == typeof(AppDbContext)))
                    .ToList();

                foreach (var descritor in descritores)
                {
                    services.Remove(descritor);
                }

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("PetCare360TestDb"));

                services.RemoveAll<IAuditoriaRepository>();
                services.AddSingleton<IAuditoriaRepository, FakeAuditoriaRepository>();

                services.Configure<HealthCheckServiceOptions>(options =>
                {
                    var mongoCheck = options.Registrations.FirstOrDefault(r => r.Name == "mongodb");
                    if (mongoCheck != null)
                    {
                        options.Registrations.Remove(mongoCheck);
                    }
                });
            });
        }
    }

    [CollectionDefinition("ApiCollection")]
    public class ApiCollection : ICollectionFixture<ApiFactoryFixture>
    {
    }
}