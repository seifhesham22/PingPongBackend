using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
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
            UserManager<User> _user,
            PingPongDbContext _db) : IRequestHandler<Command, Result>
        {
            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var userId = _currentUser.UserId;

                var requester = await _user.FindByIdAsync(userId.ToString());
                if (requester is null)
                    return Result.Failure(new Error(
                        "Friendship.RequesterNotFound",
                        "Couldn't find the current user.",
                        StatusCodes.Status404NotFound));

                var addressee = await _user.FindByNameAsync(request.userName);
                if (addressee is null)
                    return Result.Failure(new Error(
                        "Friendship.AddresseeNotFound",
                        "Couldn't find a user with that user name.",
                        StatusCodes.Status404NotFound));

                if (requester.Id == addressee.Id)
                    return Result.Failure(new Error(
                        "Friendship.SelfRequest",
                        "You can't send a friend request to yourself.",
                        StatusCodes.Status400BadRequest));

                var (first, second) = Friendship.OrderPair(requester.Id, addressee.Id);

                if (await _db.Friendships.AnyAsync(
                        f => f.FirstUserId == first && f.SecondUserId == second,
                        cancellationToken))
                    return Result.Failure(new Error(
                        "Friendship.AlreadyExists",
                        "Friendship already exists.",
                        StatusCodes.Status409Conflict));

                var friendShip = Friendship.Request(requester.Id, addressee.Id);
                _db.Friendships.Add(friendShip);
                await _db.SaveChangesAsync(cancellationToken);

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