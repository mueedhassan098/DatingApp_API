//using APIGateWay.Data;
//using APIGateWay.Entities;
//using APIGateWay.Extensions;
//using APIGateWay.Middleware;
//using APIGateWay.SignalR;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;

//var builder = WebApplication.CreateBuilder(args);
//// Add services to the container.
//builder.Services.AddControllers();
//builder.Services.AddAplicationService(builder.Configuration);
//builder.Services.AddIdentityServices(builder.Configuration);
//var app = builder.Build();


//// Configure the HTTP request pipeline.
//app.UseMiddleware<ExcptionMiddleware>();


////Configure the HTTP request pipeline.

//app.UseCors(builder => builder
//.AllowAnyHeader()
//.AllowAnyMethod()
//.AllowCredentials()
//.WithOrigins("https://localhost:4200"));


//app.UseAuthentication();
//app.UseAuthorization();

//// Serve default files and static files
//app.UseDefaultFiles();
//app.UseStaticFiles();


//app.MapControllers();

//// Map SignalR hubs
//app.MapHub<PresenceHub>("hubs/presence");
//app.MapHub<MessageHub>("hubs/message");
//app.MapFallbackToController("Index", "Fallback");


//using var scope=app.Services.CreateScope();
//var services=scope.ServiceProvider;


//try
//{
//    var context = services.GetRequiredService<DataContextClass>();
//    var userManager = services.GetRequiredService<UserManager<App_User>>();
//    var roleManageer = services.GetRequiredService<RoleManager<AppRole>>();

//    await context.Database.MigrateAsync();
//    //context.Connections.RemoveRange(context.Connections);
//    await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [Connections]");
//    await Seed.SeedUsers(userManager, roleManageer);

//}
//catch(Exception ex)
//{
//    var logger=services.GetService<ILogger<Program>>();
//    logger.LogError(ex, "An Error Occured during Migration");
//}

//app.Run();







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

// Middleware
app.UseMiddleware<ExcptionMiddleware>();

app.UseCors(builder => builder
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .WithOrigins("https://localhost:4200"));

app.UseAuthentication();
app.UseAuthorization();



app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");
app.MapFallbackToController("Index", "Fallback");

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<DataContextClass>();
    var userManager = services.GetRequiredService<UserManager<App_User>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();

    await context.Database.MigrateAsync();
    //await Seed.ClearConnection(context);
    await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [Connections]");


    //// 👇 Detect the provider and truncate accordingly
    //var dbProvider = context.Database.ProviderName;
    //if (dbProvider.Contains("SqlServer"))
    //{
    //    await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [Connections]");
    //}
    //else if (dbProvider.Contains("Npgsql"))
    //{
    //    await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Connections\"");
    //}

    //await Seed.SeedUsers(userManager, roleManager);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration or seeding.");
}

app.Run();
