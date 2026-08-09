using PingPong.API.Features.ServerFeatures.CreateServerRequest;
using PingPong.API.Features.ServerFeatures.GetMyServersRequest;

namespace PingPong.API.Features.ServerFeatures
{
    public static class ServerEndpoints
    {
        public static void MapServerEndpoints(this WebApplication app)
        {
            var serverGroup = app.MapGroup("/servers")
                .WithTags("Servers")
                .RequireAuthorization();

            CreateServer.MapEndpoint(serverGroup);
            GetMyServers.MapEndpoint(serverGroup);
        }
    }
}