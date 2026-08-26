using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PingPong.API.Data;
using PingPong.API.Domain;
using PingPong.API.Exceptions;
using PingPong.API.Features.Shared;

namespace PingPong.API.Features.FriendShipFeature.AcceptFriendShipRequest
{
    public sealed class AcceptFriendShip
    {
        public sealed record Command(Guid requesterId) : IRequest<Result>;

        public sealed class Handler(
            PingPongDbContext _db,
            ICurrentUser _currentUser) : IRequestHandler<Command, Result>
        {
            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var userExists = await _currentUser.UserExistsAsync(_currentUser.UserId);
                if (!userExists)
                    return Result.Failure(new Error(
                        "Friendship.RequesterNotFound",
                        "Couldn't find current user",
                        StatusCodes.Status404NotFound));

                var (first, second) = Friendship.OrderPair(request.requesterId, _currentUser.UserId);

                var friendship =  await _db.Friendships.FirstOrDefaultAsync(x => 
                    x.FirstUserId == first &&
                    x.SecondUserId == second &&
                    x.Status == FriendshipStatus.Pending,
                    cancellationToken);

                if(friendship is null)
                    return Result.Failure(new Error(
                        "Friendship.NotFound",
                        "Friendship request not found",
                        StatusCodes.Status404NotFound));
                try
                {
                    friendship.Accept(_currentUser.UserId);
                }
                catch (DomainException ex)
                {
                    return Result.Failure(new Error(
                        "Friendship.AcceptFailed",
                        $"Failed to accept friendship request: {ex.Message}",
                        StatusCodes.Status400BadRequest));
                }

                var chatExists = await _db.ChatMembers.AnyAsync(
                    mine => mine.UserId == _currentUser.UserId
                        && mine.Chat.ChatMembers.Any(other => other.UserId == request.requesterId)
                        && mine.Chat.ChatMembers.Count == 2,
                    cancellationToken);

                if (!chatExists)
                    _db.Chats.Add(Chat.CreateDirectChat(_currentUser.UserId, request.requesterId));

                await _db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
        }

        public static void MapEndpoint(RouteGroupBuilder group)
        {
            group.MapPost("requests/{requesterId}/accept", async (
                Guid requesterId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new Command(requesterId), cancellationToken);
                return result.Match(
                    () => Results.NoContent(),
                    error => Results.Problem(
                        title: error.Code,
                        type: error.Message,
                        statusCode: error.StatusCode)
                );
            }).WithName("AcceptFriendShipRequest");
        }
    }
}