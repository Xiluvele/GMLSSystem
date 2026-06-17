using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GMLSSystem.API.Data;
using GMLSSystem.API.Models;

namespace GMLSSystem.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add In-Memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("GLMS_TestDB");
                });

                // Build service provider and seed data
                var serviceProvider = services.BuildServiceProvider();
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    dbContext.Database.EnsureCreated();

                    // Seed test data - but NOT users (Identity handles users separately)
                    SeedTestData(dbContext);
                }
            });
        }

        private void SeedTestData(ApplicationDbContext context)
        {
            // Add test client ONLY (not users - Identity handles that)
            var client = new Client
            {
                ClientId = 1,
                Name = "Test Client",
                ContactEmail = "test@client.com",
                ContactPhone = "0123456789",
                Region = "Johannesburg",
                Address = "123 Test Street",
                CreatedAt = DateTime.UtcNow
            };

            // Only add if not already exists
            if (!context.Clients.Any())
            {
                context.Clients.Add(client);
                context.SaveChanges();
            }
        }
    }
}