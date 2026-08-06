using MediatR;
using Microsoft.EntityFrameworkCore;
using PingPong.API.Data;
using PingPong.API.Features.Shared;
using System.Reflection.Metadata.Ecma335;

namespace PingPong.API.Features.FriendShipFeature.UnblockFriendShipRequest
{
    public sealed class UnblockFriend
    {
        public sealed record Command(Guid toBeUnblocked) : IRequest<Result>;

        public sealed class Handler(ICurrentUser _currentUser, PingPongDbContext _db)
            : IRequestHandler<Command, Result>
        {
            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var blockerExists = await _currentUser.UserExistsAsync(_currentUser.UserId);
                if(!blockerExists)
                    return Result.Failure(new Error(
                        "Friendship.RequesterNotFound",
                        "Couldn't find current user.",
                        StatusCodes.Status404NotFound));

                var blockedUserExists = await _currentUser.UserExistsAsync(request.toBeUnblocked);
                if(!blockedUserExists)
                    return Result.Failure(new Error(
                        "Friendship.BlockedUserNotFound",
                        "Couldn't find the user to be unblocked.",
                        StatusCodes.Status404NotFound));

                var deleteCount = await _db.Blocks
                    .Where(b => b.BlockerId == _currentUser.UserId)
                    .Where(b => b.BlockedId == request.toBeUnblocked)
                    .ExecuteDeleteAsync(cancellationToken);

                if (deleteCount == 0)
                    return Result.Failure(new Error(
                        "Block.NotFound",
                        "You have not blocked this user",
                        StatusCodes.Status404NotFound));

                await _db.SaveChangesAsync();
                return Result.Success();
            }
        }

        public static void MapEndpoint(RouteGroupBuilder group)
        {
            group.MapPost("unblock/{toBeUnblocked}", async (
                Guid toBeUnblocked,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new Command(toBeUnblocked), cancellationToken);
                result.Match(
                    () => Results.NoContent(),
                    error => Results.Problem(
                        title: error.Code,
                        type: error.Message,
                        statusCode: error.StatusCode));
            })
            .WithName("UnBlockFriendShipRequest");
        }
    }
}