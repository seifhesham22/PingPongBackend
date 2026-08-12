using MediatR;
using Microsoft.EntityFrameworkCore;
using PingPong.API.Data;
using PingPong.API.Domain;
using PingPong.API.Features.Shared;

namespace PingPong.API.Features.FriendShipFeature.UnFriendRequest
{
    public sealed class UnFriend
    {
        public sealed record Command(Guid toBeUnfriend) : IRequest<Result>;

        public sealed class Handler(ICurrentUser _user, PingPongDbContext _db) : IRequestHandler<Command, Result>
        {
            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var currentUserId = _user.UserId;

                var (first, second) = Friendship.OrderPair(request.toBeUnfriend, currentUserId);

                var affectedRows = await _db.Friendships
                    .Where(x => x.FirstUserId == first && x.SecondUserId == second)
                    .ExecuteDeleteAsync(cancellationToken);

                if (affectedRows == 0)
                    return Result.Failure(new Error(
                        Code: "Friendship.NotFound",
                        Message: "Couldn't find a friendship between these users",
                        StatusCode: StatusCodes.Status404NotFound));

                return Result.Success();
            }
        }

        public static void MapEndpoint(RouteGroupBuilder group)
        {
            group.MapPost("/unfriend/{id}", async (ISender sender, Guid id) =>
            {
                var result = await sender.Send(new Command(id));

                return result.Match(
                    () => Results.NoContent(),
                    error => Results.Problem(
                        title: error.Code,
                        type: error.Message,
                        statusCode: error.StatusCode));
            });
        }
    }
}