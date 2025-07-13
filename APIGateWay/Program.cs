using APIGateWay.Data;
using APIGateWay.Entities;
using APIGateWay.Extensions;
using APIGateWay.Middleware;
using APIGateWay.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddAplicationService(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
var app = builder.Build();


// Configure the HTTP request pipeline.
app.UseMiddleware<ExcptionMiddleware>();


//Configure the HTTP request pipeline.

app.UseCors(builder => builder
.AllowAnyHeader()
.AllowAnyMethod()
.AllowCredentials()
.WithOrigins("https://localhost:4200"));

//app.UseCors(builder => builder
//    .AllowAnyHeader()
//    .AllowAnyMethod()
//    .AllowCredentials()
//    .WithOrigins(
//        "http://localhost:4200",  // Angular default
//        "https://localhost:4200"  // if you run Angular in https
//    ));



app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Map SignalR hubs
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");


using var scope=app.Services.CreateScope();
var services=scope.ServiceProvider;


try
{
    var context = services.GetRequiredService<DataContextClass>();
    var userManager = services.GetRequiredService<UserManager<App_User>>();
    var roleManageer = services.GetRequiredService<RoleManager<AppRole>>();

    await context.Database.MigrateAsync();
    //context.Connections.RemoveRange(context.Connections);
    await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [Connections]");
    await Seed.SeedUsers(userManager, roleManageer);

}
catch(Exception ex)
{
    var logger=services.GetService<ILogger<Program>>();
    logger.LogError(ex, "An Error Occured during Migration");
}

app.Run();
