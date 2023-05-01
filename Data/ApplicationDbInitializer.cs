using Microsoft.AspNetCore.Identity;
using Application.Models;

namespace Application.Data;

public class ApplicationDbInitializer
{
    public static async Task SeedUsers(IApplicationBuilder app)
    {
        using (var serviceScope = app.ApplicationServices.CreateScope())
        {
            var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            if (!userManager.Users.Any())
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));

                var admin = new User
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                };
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, "Admin");

                await roleManager.CreateAsync(new IdentityRole("User"));

                var user1 = new User
                {
                    UserName = "user1@example.com",
                    Email = "user1@example.com",
                };
                await userManager.CreateAsync(user1, "User1@123");
                await userManager.AddToRoleAsync(user1, "User");

                var user2 = new User
                {
                    UserName = "user2@example.com",
                    Email = "user2@example.com",
                };
                await userManager.CreateAsync(user2, "User2@123");
                await userManager.AddToRoleAsync(user2, "User");
            }
        }
    }
}