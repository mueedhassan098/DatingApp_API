using APIGateWay.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace APIGateWay.Data
{
    public class Seed
    {
        public static async Task SeedUsers(DataContextClass contextClass)
        {
            if (await contextClass.Users.AnyAsync()) return;
            var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var users = JsonSerializer.Deserialize<List<App_User>>(userData);
            foreach ( var user in users )
            {
                using var hmac=new HMACSHA512();
                user.UserName=user.UserName.ToLower();
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("pa$$w0rd"));
                user.PasswordSalt = hmac.Key;
                contextClass.Users.Add(user);
            }
            await contextClass.SaveChangesAsync();
        }
    }
}
