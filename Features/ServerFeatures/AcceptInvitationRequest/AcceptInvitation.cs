using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PingPong.API.Data;
using PingPong.API.Exceptions;
using PingPong.API.Features.Shared;

namespace PingPong.API.Features.ServerFeatures.AcceptInvitationRequest
{
    public sealed class AcceptInvitation
    {
        public sealed record Command(string InvitationToken) : IRequest<Result>;

        public sealed class Handler(ICurrentUser _currentUser, PingPongDbContext _db) : IRequestHandler<Command, Result>
        {
            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var CurrentUserId = _currentUser.UserId;

                var server = await _db.Servers
                    .Include(s => s.ServerInvitations.Where(i => i.Token == request.InvitationToken))
                    .Include(s => s.Memberships.Where(i => i.UserId == CurrentUserId))
                    .FirstOrDefaultAsync(s => s.ServerInvitations.Any(x => x.Token == request.InvitationToken), cancellationToken);

                if (server is null)
                    return Result.Failure(new Error(
                        "Server.InvitationCodeNotFound",
                        "Couldn't find an invitation assosiated to this invitation code",
                        StatusCodes.Status400BadRequest));

                try
                {
                    server.AcceptInvitation(CurrentUserId, request.InvitationToken);
                }
                catch (DomainException ex)
                {
                    return Result.Failure(new Error(
                        $"{ex.Message}",
                        "can't join this server, please try again later",
                        StatusCodes.Status400BadRequest));
                }

                await _db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
        }

        public static void MapEndpoint(RouteGroupBuilder group)
        {
            group.MapPost("/join", async ([FromBody] string token, ISender sender) =>
            {
                var result = await sender.Send(new Command(token));

                return result.Match(
                    () => Results.NoContent(),
                    error => Results.Problem(
                        type:error.Code,
                        title: error.Message,
                        statusCode: error.StatusCode));
            }).WithName("AcceptServerInvitation");
        }
    }
}