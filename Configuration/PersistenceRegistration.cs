using Microsoft.EntityFrameworkCore;
using PingPong.API.Data;

namespace PingPong.API.Configuration
{
    public static class PersistenceRegistration
    {
        public static IServiceCollection AddPersistence(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PingPongDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
                options
           .LogTo(Console.WriteLine, LogLevel.Information)
           .EnableSensitiveDataLogging()   // shows actual parameter values, not just @p0, @p1...
           .EnableDetailedErrors();
            }
                );

            return services;
        }
    }
}