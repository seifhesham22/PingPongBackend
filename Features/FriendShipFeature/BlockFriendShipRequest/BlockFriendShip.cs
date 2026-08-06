using MediatR;
using Microsoft.EntityFrameworkCore;
using PingPong.API.Data;
using PingPong.API.Domain;
using PingPong.API.Exceptions;
using PingPong.API.Features.Shared;

namespace PingPong.API.Features.FriendShipFeature.BlockFriendShipRequest
{
    public sealed class BlockFriendShip
    {
        public sealed record Command(Guid toBeBlocked) : IRequest<Result>;
        public sealed class Handler(ICurrentUser _currentUser, PingPongDbContext _db) : IRequestHandler<Command, Result>
        {
            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var userExists = await _currentUser.UserExistsAsync(_currentUser.UserId);
                if (!userExists)
                    return Result.Failure(new Error(
                        "Friendship.RequesterNotFound",
                        "Couldn't find current user.",
                        StatusCodes.Status404NotFound));

                var (first, second) = Friendship.OrderPair(_currentUser.UserId, request.toBeBlocked);

                var friendship = await _db.Friendships
                    .FirstOrDefaultAsync(x => x.FirstUserId == first && x.SecondUserId == second);

                if (friendship == null)
                    return Result.Failure(new Error(
                        "Friendship.NotFound",
                        "Couldn't find friendship.",
                        StatusCodes.Status404NotFound));

                try
                {
                    friendship.Block(_currentUser.UserId);
                }
                catch (DomainException ex)
                {
                    return Result.Failure(new Error(
                        "Friendship.BlockFailed",
                        $"Failed to block friendship: {ex.Message}",
                        StatusCodes.Status409Conflict));
                }
                await _db.SaveChangesAsync();
                return Result.Success();
            }
        }

        public static void MapEndpoint(RouteGroupBuilder group)
        {
            group.MapPost("block/{toBeBlocked}", async (
                Guid toBeBlocked,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new Command(toBeBlocked), cancellationToken);
                return result.Match(
                    () => Results.NoContent(),
                    error => Results.Problem(
                        title: error.Code,
                        type: error.Message,
                        statusCode: error.StatusCode));
            })
            .WithName("BlockFriendShipRequest");
        }
    }
}