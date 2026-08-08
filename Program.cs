using PingPong.API.Configuration;
using PingPong.API.Features.Authentication;
using PingPong.API.Features.FriendShipFeature;
using PingPong.API.Features.ServerFeatures;

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapAuthenticationEndpoints();
app.MapFriendShipEndpoints();
app.MapServerEndpoints();

app.Run();