using MediatR;
using Microsoft.EntityFrameworkCore;
using PingPong.API.Data;
using PingPong.API.Domain;
using PingPong.API.Features.Shared;

namespace PingPong.API.Features.ServerFeatures.GetServerByIdRequest
{
    public sealed class GetServer
    {
        public sealed record ChannelDto(Guid id, string name, int position, string type);
        public sealed record ChannelGroupDto(Guid id, string name, int position, List<ChannelDto> channels);
        public sealed record ServerDto(
            Guid id,
            string name,
            Guid? serverIcon,
            List<ChannelDto> ungroupedChannels,
            List<ChannelGroupDto> channelGroups);

        public sealed record Query(Guid id) : IRequest<Result<ServerDto>>;

        public sealed class Handler(PingPongDbContext _db, ICurrentUser _currentUser) : IRequestHandler<Query, Result<ServerDto>>
        {
            public async Task<Result<ServerDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var userId = _currentUser.UserId;

                var server = await _db.Servers.AsNoTracking()
                    .Where(s => s.Id == request.id && s.Memberships.Any(x => x.ServerId == request.id
                            && x.UserId == userId))
                    .Select(x => new ServerDto(
                        id: x.Id,
                        name: x.Name,
                        serverIcon: x.ServerIcon,
                        //Ungrouped channels
                        ungroupedChannels: x.Channels
                        .Where(c => c.GroupId == null)
                        .OrderBy(p => p.Position)
                        .Select(c => new ChannelDto(
                            id: c.Id,
                            name: c.Name,
                            position: c.Position,
                            type: c is TextChannel ? "Text" : "Voice")).ToList(),
                        channelGroups: x.ChannelGroups.OrderBy(p => p.Position)
                        //Grouped channels
                        .Select(g => new ChannelGroupDto(
                            id: g.Id,
                            name: g.Name,
                            position: g.Position,
                            channels: g.Channels.OrderBy(p => p.Position).Select(c => new ChannelDto(
                                id: c.Id,
                                name: c.Name,
                                position: c.Position,
                                type: c is TextChannel ? "Text" : "Voice"))
                            .ToList()))
                        .ToList()))
                    .FirstOrDefaultAsync(cancellationToken);

                if (server is null)
                    return new Error(
                        Code: "Server.NotFound",
                        Message: "Couldn't find a server with the provided id",
                        StatusCode: StatusCodes.Status404NotFound);

                return Result<ServerDto>.Success(server);
            }
        }

        public static void MapEndpoint(RouteGroupBuilder group)
        {
            group.MapGet("/{id:guid}", async (
                ISender sender,
                Guid id,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new Query(id), cancellationToken);
                return result.Match(
                    value => Results.Ok(value),
                    error => Results.Problem(
                        type: error.Code,
                        title: error.Message,
                        statusCode: error.StatusCode));
            }).WithName("GetServerById");
        }
    }
}