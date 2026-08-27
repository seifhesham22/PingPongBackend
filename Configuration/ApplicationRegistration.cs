using PingPong.API.Features.Shared;

namespace PingPong.API.Configuration
{
    public static class ApplicationRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddHttpContextAccessor();

            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
            services.AddAntiforgery();
            services.AddSignalR();
            services.AddControllers();

            return services;
        }
    }
}