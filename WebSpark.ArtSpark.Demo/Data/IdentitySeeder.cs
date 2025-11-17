using Microsoft.AspNetCore.Identity;
using WebSpark.ArtSpark.Demo.Models;

namespace WebSpark.ArtSpark.Demo.Data;

public static class IdentitySeeder
{
    public static async Task SeedRolesAndAdminAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger logger)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Seed roles
        var defaultRoles = configuration.GetSection("Identity:DefaultRoles").Get<string[]>() ?? new[] { "User", "Admin" };

        foreach (var roleName in defaultRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (result.Succeeded)
                {
                    logger.LogInformation("Created role: {RoleName}", roleName);
                }
                else
                {
                    logger.LogError("Failed to create role {RoleName}: {Errors}",
                        roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        // Seed bootstrap admin if configured
        var adminEmail = configuration["Identity:BootstrapAdmin:Email"];
        var adminPassword = configuration["Identity:BootstrapAdmin:Password"];
        var adminDisplayName = configuration["Identity:BootstrapAdmin:DisplayName"] ?? "Administrator";

        if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword))
        {
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    DisplayName = adminDisplayName,
                    EmailConfirmed = true,
                    EmailVerified = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    await userManager.AddToRoleAsync(adminUser, "User");
                    logger.LogInformation("Bootstrap admin user created: {Email}", adminEmail);
                }
                else
                {
                    logger.LogError("Failed to create bootstrap admin: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                // Ensure existing admin has Admin role
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation("Added Admin role to existing user: {Email}", adminEmail);
                }
            }
        }

        // Assign Admin role to all existing users
        var allUsers = userManager.Users.ToList();
        foreach (var user in allUsers)
        {
            if (!await userManager.IsInRoleAsync(user, "Admin"))
            {
                await userManager.AddToRoleAsync(user, "Admin");
                logger.LogInformation("Added Admin role to existing user: {Email}", user.Email);
            }

            if (!await userManager.IsInRoleAsync(user, "User"))
            {
                await userManager.AddToRoleAsync(user, "User");
                logger.LogInformation("Added User role to existing user: {Email}", user.Email);
            }
        }
    }
}
