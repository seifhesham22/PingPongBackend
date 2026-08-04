using MediatR;
using Microsoft.EntityFrameworkCore;
using PingPong.API.Data;
using PingPong.API.Domain;
using PingPong.API.Features.Shared;

namespace PingPong.API.Features.FriendShipFeature.RejectFriendShipRequest
{
    public sealed class RejectFriendShip
    {
        public sealed record Command(Guid requesterId) : IRequest<Result>;
        public sealed class Handler(ICurrentUser _currentUser, PingPongDbContext _db) : IRequestHandler<Command, Result>
        {
            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var userExists = await _currentUser.UserExistsAsync(_currentUser.UserId);
                if(!userExists)
                return Result.Failure(new Error(
                    "Friendship.RequesterNotFound",
                    "Couldn't find current user",
                    StatusCodes.Status404NotFound));

                var (first, second) = Friendship.OrderPair(_currentUser.UserId, request.requesterId);
                var friendShip = await _db.Friendships.FirstOrDefaultAsync(
                    x => x.FirstUserId == first &&
                    x.SecondUserId == second &&
                    x.RequesterId == request.requesterId &&
                    x.Status == FriendshipStatus.Pending,
                    cancellationToken);

                if(friendShip == null)
                return Result.Failure(new Error(
                    "Friendship.FriendshipNotFound",
                    "friendship request not found",
                    StatusCodes.Status404NotFound));

                _db.Friendships.Remove(friendShip);
                try
                {
                    await _db.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    return Result.Failure(new Error(
                   "Friendship.FriendshipNotFound",
                   "friendship request not found",
                   StatusCodes.Status404NotFound));
                }
                return Result.Success();
            }
        }
        public static void MapEndpoint(RouteGroupBuilder group)
        {
            group.MapPost("requests/{requesterId}/reject", async (
                Guid requesterId,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new Command(requesterId));
                return result.Match(
                    () => Results.NoContent(),
                    error => Results.Problem(
                        title: error.Code,
                        type: error.Message,
                        statusCode: error.StatusCode));
            })
            .WithName("RejectFriendShipRequest");
        }
    }
}