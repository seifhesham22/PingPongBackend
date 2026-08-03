using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using PingPong.API.Data;
using PingPong.API.Domain;
using PingPong.API.Features.Authentication;
using PingPong.API.Features.FriendShipFeature.AddNewFriend;
using PingPong.API.Features.FriendShipFeature.GetMyFriendShipRequests;
using PingPong.API.Features.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PingPongDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddIdentityApiEndpoints<User>(options =>
{
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultPhoneProvider;
})
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<PingPongDbContext>()
    .AddTokenProvider<PhoneNumberTokenProvider<User>>(TokenOptions.DefaultPhoneProvider);

builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IEmailSender<User>,IdentityEmailSender>();

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var friendsGroup = app.MapGroup("/friends")
    .WithTags("Friends")
    .RequireAuthorization();

AddFriend.MapEndpoint(friendsGroup);
GetFriendRequests.MapEndpoint(friendsGroup);

var identityGroup = app.MapGroup("/auth")
    .WithTags("Identity");

identityGroup.MapGet("/check2fa", async (string email, UserManager<User> userManager) =>
{
    if (string.IsNullOrEmpty(email))
        return Results.BadRequest("Email can't be empty");

    var user = await userManager.FindByEmailAsync(email);
    if (user == null) return Results.NotFound($"Couldn't find a user with Email: {email}");

    var has2fa = await userManager.GetTwoFactorEnabledAsync(user);
    if (has2fa)
        return Results.Ok("enabled");

    return Results.Ok("not enabled");
});
identityGroup.MapIdentityApi<User>();
app.Run();