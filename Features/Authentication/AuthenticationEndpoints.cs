using Microsoft.AspNetCore.Identity;
using PingPong.API.Domain;

namespace PingPong.API.Features.Authentication
{
    public static class AuthenticationEndpoints
    {
        public static void MapAuthenticationEndpoints(this WebApplication app)
        {
            var identityGroup = app.MapGroup("/auth")
                .WithTags("Identity");

            identityGroup.MapGet("/check2fa", async (
                string email,
                UserManager<User> userManager) =>
            {
                if (string.IsNullOrEmpty(email))
                    return Results.BadRequest("Email can't be empty");

                var user = await userManager.FindByEmailAsync(email);
                if (user is null)
                    return Results.NotFound($"Couldn't find a user with Email: {email}");

                var has2fa = await userManager.GetTwoFactorEnabledAsync(user);

                return Results.Ok(has2fa ? "enabled" : "not enabled");
            })
            .WithName("Check2Fa");

            identityGroup.MapIdentityApi<User>();
        }
    }
}