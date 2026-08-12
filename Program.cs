using PingPong.API.Configuration;
using PingPong.API.Features.Authentication;
using PingPong.API.Features.FriendShipFeature;
using PingPong.API.Features.ServerFeatures;
using PingPong.API.Features.ChatFeatures;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPersistence(builder.Configuration)
    .AddAuthenticationSetup()
    .AddApplicationServices()
    .AddSwaggerWithBearerAuth();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Must come before UseAuthentication: it rewrites the hub handshake's query-string
// token into a header the Identity bearer scheme can read.
app.UseHubAuthentication();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapAuthenticationEndpoints();
app.MapFriendShipEndpoints();
app.MapServerEndpoints();
app.MapChatEndpoints();

app.Run();