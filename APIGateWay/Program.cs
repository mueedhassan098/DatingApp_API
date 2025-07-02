using APIGateWay.Data;
using APIGateWay.Entities;
using APIGateWay.Extensions;
using APIGateWay.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddAplicationService(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
var app = builder.Build();
app.UseMiddleware<ExcptionMiddleware>();
// Configure the HTTP request pipeline.
app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200"));
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using var scope=app.Services.CreateScope();
var services=scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<DataContextClass>();
    var userManager = services.GetRequiredService<UserManager<App_User>>();
    var roleManageer = services.GetRequiredService<RoleManager<AppRole>>();

    await context.Database.MigrateAsync();
    await Seed.SeedUsers(userManager, roleManageer);

}
catch(Exception ex)
{
    var logger=services.GetService<ILogger<Program>>();
    logger.LogError(ex, "An Error Occured during Migration");
}

app.Run();
