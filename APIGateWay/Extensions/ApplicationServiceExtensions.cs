using APIGateWay.Data;
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
            return services;
        }
    }
}
