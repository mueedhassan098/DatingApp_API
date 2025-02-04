using APIGateWay.Controllers;
using APIGateWay.Entities;
using Microsoft.EntityFrameworkCore;

namespace APIGateWay.Data
{
    public class DataContextClass:DbContext
    {
        public DataContextClass(DbContextOptions options) : base(options)
        {
            
        }
        
        public DbSet<App_User> Users { get; set; }
    }
    
}
