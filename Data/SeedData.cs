using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using GMLSSystem.Models;

namespace GMLSSystem.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Ensure database is created
                await context.Database.EnsureCreatedAsync();

                // Create roles
                foreach (var role in UserRoles.AllRoles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                        Console.WriteLine($"Created role: {role}");
                    }
                }

                // Create Admin user
                var adminEmail = "admin@glms.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = "admin",
                        Email = adminEmail,
                        FullName = "System Administrator",
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin@123");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
                        Console.WriteLine("Created Admin user");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"Error creating admin: {error.Description}");
                        }
                    }
                }

                // Create sample Contract Manager
                var managerEmail = "manager@glms.com";
                var managerUser = await userManager.FindByEmailAsync(managerEmail);

                if (managerUser == null)
                {
                    managerUser = new ApplicationUser
                    {
                        UserName = "contractmanager",
                        Email = managerEmail,
                        FullName = "John Manager",
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(managerUser, "Manager@123");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(managerUser, UserRoles.ContractManager);
                        Console.WriteLine("Created Contract Manager user");
                    }
                }

                // Create sample Logistics Coordinator
                var logisticsEmail = "logistics@glms.com";
                var logisticsUser = await userManager.FindByEmailAsync(logisticsEmail);

                if (logisticsUser == null)
                {
                    logisticsUser = new ApplicationUser
                    {
                        UserName = "logistics",
                        Email = logisticsEmail,
                        FullName = "Sarah Logistics",
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(logisticsUser, "Logistics@123");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(logisticsUser, UserRoles.LogisticsCoordinator);
                        Console.WriteLine("Created Logistics Coordinator user");
                    }
                }

                // Create sample Finance User
                var financeEmail = "finance@glms.com";
                var financeUser = await userManager.FindByEmailAsync(financeEmail);

                if (financeUser == null)
                {
                    financeUser = new ApplicationUser
                    {
                        UserName = "finance",
                        Email = financeEmail,
                        FullName = "Mike Finance",
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(financeUser, "Finance@123");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(financeUser, UserRoles.FinanceUser);
                        Console.WriteLine("Created Finance user");
                    }
                }

                // Create sample Client user (will link to client after client is created)
                var clientEmail = "client@abclogistics.com";
                var clientUser = await userManager.FindByEmailAsync(clientEmail);

                if (clientUser == null)
                {
                    clientUser = new ApplicationUser
                    {
                        UserName = "abccorp",
                        Email = clientEmail,
                        FullName = "ABC Corporation",
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(clientUser, "Client@123");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(clientUser, UserRoles.Client);
                        Console.WriteLine("Created Client user");
                    }
                }

                Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                    SEEDING COMPLETE                      ║");
                Console.WriteLine("╠══════════════════════════════════════════════════════════╣");
                Console.WriteLine("║ Admin:              admin / Admin@123                    ║");
                Console.WriteLine("║ Contract Manager:   contractmanager / Manager@123        ║");
                Console.WriteLine("║ Logistics:          logistics / Logistics@123            ║");
                Console.WriteLine("║ Finance:            finance / Finance@123                ║");
                Console.WriteLine("║ Client:             abccorp / Client@123                 ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
            }
        }
    }
}