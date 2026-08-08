using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using PingPong.API.Data;
using PingPong.API.Features.Shared;
using PingPong.API.Features.Shared.Pagination;

namespace PingPong.API.Features.ServerFeatures.GetMyServersRequest
{
    public class GetMyServers
    {
        public sealed record ServerDto(Guid id, string name, Guid? serverIconId);
        public sealed record Query() : IRequest<List<ServerDto>>;

        public sealed class Handler(PingPongDbContext _db, ICurrentUser _currentUser)
            : IRequestHandler<Query, List<ServerDto>>
        {
            public async Task<List<ServerDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var userId = _currentUser.UserId;

                var servers = await _db.UserServers
                    .AsNoTracking()
                    .Where(x => x.UserId == userId)
                    .OrderBy(x => x.JoinedAt)
                    .Select(x => new ServerDto(
                        id: x.ServerId,
                        name: x.Server.Name,
                        serverIconId: x.Server.ServerIcon))
                    .ToListAsync(cancellationToken);

                return servers;
            }
        }

        public static void MapEndpoint(RouteGroupBuilder group)
        {
            group.MapGet("/my", async (ISender sender, CancellationToken cancellationToken) =>
            {
                return Results.Ok(await sender.Send(new Query(), cancellationToken));
            });
        }
    }
}