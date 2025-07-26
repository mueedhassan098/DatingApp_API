using APIGateWay.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace APIGateWay.Data
{
    public class Seed
    {

        //public static async Task ClearConnection(DataContextClass dataContext)
        //{
        //    dataContext.Connections.RemoveRange(dataContext.Connections);
        //    await dataContext.SaveChangesAsync();
        //}
        public static async Task SeedUsers(UserManager<App_User> userManager, RoleManager<AppRole> roleManager)
        {
            if (await userManager.Users.AnyAsync()) return;
          
            var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");           
            var users = JsonSerializer.Deserialize<List<App_User>>(userData);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var roles = new List<AppRole>
               {
                   new AppRole { Name = "Member" },
                   new AppRole { Name = "Admin" },
                   new AppRole { Name = "Moderator" }
               };

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            }

            foreach (var user in users)
            {
                user.Photos.First().IsApproved = true; // Assuming the first photo is the main photo and should be approved
                user.UserName = user.UserName.ToLower();
                user.Created = DateTime.SpecifyKind(user.Created, DateTimeKind.Utc);
                user.LastActive = DateTime.SpecifyKind(user.LastActive, DateTimeKind.Utc);

                await userManager.CreateAsync(user, "Pa$$w0rd");
                await userManager.AddToRoleAsync(user, "Member");

            }
            var admin = new App_User
            {
                UserName = "admin",
            };

            await userManager.CreateAsync(admin, "Pa$$w0rd");
            await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
        }
    }
}


//using APIGateWay.Entities;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using System.Text.Json;

//namespace APIGateWay.Data
//{
//    public class Seed
//    {
//        public static async Task ClearConnection(DataContextClass dataContext)
//        {
//            dataContext.Connections.RemoveRange(dataContext.Connections);
//            await dataContext.SaveChangesAsync();
//        }
//        public static async Task SeedUsers(UserManager<App_User> userManager, RoleManager<AppRole> roleManager)
//        {
//            if (await userManager.Users.AnyAsync()) return;

//            string userData = string.Empty;
//            try
//            {
//                userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("File read error: " + ex.Message);
//                return;
//            }

//            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
//            var users = JsonSerializer.Deserialize<List<App_User>>(userData, options);

//            if (users == null || users.Count == 0)
//            {
//                Console.WriteLine("No users found in JSON file.");
//                return;
//            }

//            var roles = new List<AppRole>
//            {
//                new AppRole { Name = "Member" },
//                new AppRole { Name = "Admin" },
//                new AppRole { Name = "Moderator" }
//            };

//            foreach (var role in roles)
//            {
//                var result = await roleManager.CreateAsync(role);
//                if (!result.Succeeded)
//                {
//                    Console.WriteLine($"Role creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
//                }
//            }

//            foreach (var user in users)
//            {
//                if (user.Photos?.Any() == true)
//                    user.Photos.First().IsApproved = true;

//                user.UserName = user.UserName.ToLower();

//                var result = await userManager.CreateAsync(user, "Pa$$w0rd");
//                if (!result.Succeeded)
//                {
//                    Console.WriteLine($"User creation failed for {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
//                    continue;
//                }

//                await userManager.AddToRoleAsync(user, "Member");
//            }

//            var admin = new App_User
//            {
//                UserName = "admin"
//            };

//            var adminResult = await userManager.CreateAsync(admin, "Pa$$w0rd");
//            if (adminResult.Succeeded)
//            {
//                await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
//            }
//            else
//            {
//                Console.WriteLine($"Admin creation failed: {string.Join(", ", adminResult.Errors.Select(e => e.Description))}");
//            }
//        }
//    }
//}
