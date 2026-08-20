using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PingPong.API.Data;
using PingPong.API.Domain;
using PingPong.API.Features.Authentication;

namespace PingPong.API.Configuration
{
    public static class AuthenticationRegistration
    {
        public static IServiceCollection AddAuthenticationSetup(this IServiceCollection services)
        {
            services.AddIdentityApiEndpoints<User>(options =>
            {
                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultPhoneProvider;
                options.User.RequireUniqueEmail = true;
            })
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<PingPongDbContext>()
                .AddTokenProvider<PhoneNumberTokenProvider<User>>(TokenOptions.DefaultPhoneProvider);


            services.Replace(ServiceDescriptor.Scoped<UserManager<User>, CustomUserManager>());
            services.AddTransient<IEmailSender<User>, IdentityEmailSender>();
            
            return services;
        }
    }
}