using MediatR;
using PingPong.API.Data;
using PingPong.API.Domain;
using PingPong.API.Features.Shared;

namespace PingPong.API.Features.ServerFeatures.CreateServerRequest
{
    public sealed class CreateServer
    {
        public sealed record CreateServerDto(string serverName, Guid? serverIcon);
        public sealed record ServerCreatedDto(Guid serverId, string serverName, Guid? serverIcon);
        public sealed record Command(string serverName, Guid? serverIcon) : IRequest<Result<ServerCreatedDto>>;

        public sealed class Handler(ICurrentUser _user, PingPongDbContext _db) : IRequestHandler<Command, Result<ServerCreatedDto>>
        {
            public async Task<Result<ServerCreatedDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var userId = _user.UserId;

                var server = Server.Create(request.serverName, userId, request.serverIcon);

                var textChannelGroup = new ChannelGroup(
                    serverId: server.Id,
                    name: ServerConstants.TextChannelGroup,
                    position: ServerConstants.ZeroPosition);

                var voiceChannelGroup = new ChannelGroup(
                    serverId: server.Id,
                    name: ServerConstants.VoiceChannelGroup,
                    position: ServerConstants.OnePosition);

                var textChannel = new TextChannel(
                    serverId: server.Id,
                    groupId: textChannelGroup.Id,
                    name: ServerConstants.TextChannel,
                    position: ServerConstants.ZeroPosition);

                var voiceChannel = new VoiceChannel(
                    serverId: server.Id,
                    groupId: voiceChannelGroup.Id,
                    ServerConstants.VoiceChannel,
                    position: ServerConstants.ZeroPosition);

                textChannelGroup.AddChannel(textChannel);
                voiceChannelGroup.AddChannel(voiceChannel);

                server.AddChannelGroup(textChannelGroup);
                server.AddChannelGroup(voiceChannelGroup);

                _db.Add(server);
                await _db.SaveChangesAsync(cancellationToken);

                return Result<ServerCreatedDto>.Success(new ServerCreatedDto(
                    serverId: server.Id, serverName: server.Name, serverIcon: server.ServerIcon));
            }
        }

        public static void MapEndpoint(RouteGroupBuilder group)
        {
            group.MapPost("/create", async (
                CreateServerDto server,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new Command(server.serverName, server.serverIcon), cancellationToken);

                return result.Match(
                    value => Results.Ok(value),
                    error => Results.Problem(
                        title: error.Message,
                        type: error.Code,
                        statusCode: error.StatusCode));
            })
            .WithName("CreateServer");
        }
    }

    public static class ServerConstants
    {
        public const string TextChannelGroup = "Text Channels";
        public const string VoiceChannelGroup = "Voice Channels";
        public const string TextChannel = "general";
        public const string VoiceChannel = "General";
        public const int ZeroPosition =  0;
        public const int OnePosition = 1;
    }
}