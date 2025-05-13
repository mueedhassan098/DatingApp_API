using APIGateWay.Data;
using APIGateWay.Helpers;
using APIGateWay.Interfaces;
using APIGateWay.Services;
using Microsoft.EntityFrameworkCore;

namespace APIGateWay.Extensions
{
    public static class ApplicationServiceExtensions
    {
        //this is a dbcontext congiguration
        public static IServiceCollection AddAplicationService(this IServiceCollection services,IConfiguration cofig)
        {
            services.AddDbContext<DataContextClass>(opt =>
            {
                opt.UseSqlServer(cofig.GetConnectionString("DefaultConnection"));
            });
            services.AddCors();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserRepository,UserRepositroy>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.Configure<CloudinarySetting>(cofig.GetSection("CloudinarySetting"));
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<LogUserActivity>();
            return services;
        }
    }
}
