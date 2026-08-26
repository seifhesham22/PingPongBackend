using PingPong.API.Features.ServerFeatures.AcceptInvitationRequest;
using PingPong.API.Features.ServerFeatures.AssignRoleRequest;
using PingPong.API.Features.ServerFeatures.RemoveRoleRequest;
using PingPong.API.Features.ServerFeatures.CreateRoleRequest;
using PingPong.API.Features.ServerFeatures.CreateServerRequest;
using PingPong.API.Features.ServerFeatures.DeleteRoleRequest;
using PingPong.API.Features.ServerFeatures.GenerateServerInviteLink;
using PingPong.API.Features.ServerFeatures.GetMyServersRequest;
using PingPong.API.Features.ServerFeatures.GetRolesRequest;
using PingPong.API.Features.ServerFeatures.GetServerByIdRequest;
using PingPong.API.Features.ServerFeatures.UpdateRoleRequest;

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
            GetServer.MapEndpoint(serverGroup);
            GenerateLink.MapEndpoints(serverGroup);
            AcceptInvitation.MapEndpoint(serverGroup);
            GetRoles.MapEndpoint(serverGroup);
            CreateRole.MapEndpoint(serverGroup);
            DeleteRole.MapEndpoint(serverGroup);
            UpdateRole.MapEndpoint(serverGroup);
            AssignRole.MapEndpoint(serverGroup);
            RemoveRole.MapEndpoint(serverGroup);  
        }
    }
}