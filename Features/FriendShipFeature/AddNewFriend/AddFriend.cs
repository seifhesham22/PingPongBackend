using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PingPong.API.Data;
using PingPong.API.Domain;
using PingPong.API.Features.Shared;

namespace PingPong.API.Features.FriendShipFeature.AddNewFriend
{
    public class AddFriend
    {
        public sealed record Command(string userName) : IRequest<Result>;

        public sealed class Handler(
            ICurrentUser _currentUser,
            UserManager<User> _userManager,
            PingPongDbContext _db) : IRequestHandler<Command, Result>
        {
            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var requesterExists = await _currentUser.UserExistsAsync(_currentUser.UserId);
                if (!requesterExists)
                    return Result.Failure(new Error(
                        "Friendship.RequesterNotFound",
                        "Couldn't find the current user.",
                        StatusCodes.Status404NotFound));

                var addressee = await _userManager.FindByNameAsync(request.userName);
                if (addressee is null)
                    return Result.Failure(new Error(
                        "Friendship.AddresseeNotFound",
                        "Couldn't find a user with that user name.",
                        StatusCodes.Status404NotFound));

                var BlockedByRequester = await _db.Blocks.AnyAsync(
                    b => b.BlockerId == _currentUser.UserId &&
                    b.BlockedId == addressee.Id,
                    cancellationToken);

                if (BlockedByRequester)
                    return Result.Failure(new Error(
                        "Friendship.IsBlocked",
                        "You have blocked this user. Unblock him to send a friend request.",
                        StatusCodes.Status403Forbidden));

                var RequesterBlockedByAddressee = await _db.Blocks.AnyAsync(
                    b => b.BlockerId == addressee.Id &&
                    b.BlockedId == _currentUser.UserId,
                    cancellationToken);
                
                if (RequesterBlockedByAddressee)
                    return Result.Failure(new Error(
                        "Friendship.IsBlocked",
                        "Failed to add friend.",
                        StatusCodes.Status403Forbidden));

                var (first, second) = Friendship.OrderPair(_currentUser.UserId, addressee.Id);

                if (await _db.Friendships.AnyAsync(
                        f => f.FirstUserId == first && f.SecondUserId == second,
                        cancellationToken))
                    return Result.Failure(new Error(
                        "Friendship.AlreadyExists",
                        "Friendship already exists.",
                        StatusCodes.Status409Conflict));

                var friendShip = Friendship.Request(_currentUser.UserId, addressee.Id);
                _db.Friendships.Add(friendShip);

                try
                {
                    await _db.SaveChangesAsync(cancellationToken);

                }
                catch (DbUpdateException ex)
                when (ex.InnerException is Npgsql.PostgresException
                { SqlState : PostgresErrorCodes.UniqueViolation })
                {
                    return Result.Failure(new Error(
                        "Friendship.AlreadyExists",
                        "Friendship already exists",
                        StatusCodes.Status409Conflict));
                }
                return Result.Success();
            }
        }

        public static void MapEndpoint(RouteGroupBuilder group)
        {
            group.MapPost("/requests", async (
                Command command,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(command, cancellationToken);

                return result.Match(
                    () => Results.NoContent(),
                    error => Results.Problem(
                        title: error.Message,
                        statusCode: error.StatusCode,
                        type: error.Code));
            })
            .WithName("AddFriend");
        }
    }
}