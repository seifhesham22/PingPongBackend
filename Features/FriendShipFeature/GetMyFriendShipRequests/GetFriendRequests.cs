using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PingPong.API.Data;
using PingPong.API.Domain;
using PingPong.API.Features.Shared;
using PingPong.API.Features.Shared.Pagination;

namespace PingPong.API.Features.FriendShipFeature.GetMyFriendShipRequests
{
    public class GetFriendRequests
    {
        public sealed record FriendshipRequestDto
        {
            public Guid RequesterId { get; init; }
            public string RequesterUserName { get; init; } = string.Empty;
            public Guid? AvaterFileId {  get; init; }
        }
        public sealed record Query : PagedRequest, IRequest<Result<List<FriendshipRequestDto>>>;
        public sealed class Handler(
            ICurrentUser _currentUser,
            PingPongDbContext _db) : IRequestHandler<Query, Result<List<FriendshipRequestDto>>>
        {
            public async Task<Result<List<FriendshipRequestDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var userId = _currentUser.UserId;

                var existingUser = await _db.Users.FindAsync(userId);
                if(existingUser is null)
                    return new Error("UserNotFound", "User not found.", 404);

                var friendRequests = await _db.Friendships
                    .Where(f => f.RequesterId != userId 
                    && f.Status == FriendshipStatus.Pending
                    && (f.FirstUserId == userId || f.SecondUserId == userId))
                    .Select(f => new FriendshipRequestDto
                    {
                        RequesterId = f.RequesterId,
                        RequesterUserName = f.RequesterId == f.FirstUserId ? f.FirstUser.UserName : f.SecondUser.UserName,
                        AvaterFileId = f.RequesterId == f.FirstUserId ? f.FirstUser.AvaterFileId : f.SecondUser.AvaterFileId,
                    })
                    .ToListAsync(cancellationToken);

                return Result<List<FriendshipRequestDto>>.Success(friendRequests);
            }
        }
        public static void MapEndpoint(RouteGroupBuilder endpoints)
        {
            endpoints.MapGet("/requests", async ([AsParameters] Query query, ISender sender) =>
            {
                var result = await sender.Send(query);
                return result.Match(
                    value => Results.Ok(result.Value),
                    error => Results.Problem(
                        title: error.Message,
                        statusCode: error.StatusCode,
                        type: error.Code));
            })
            .WithName("GetFriendRequests")
            .RequireAuthorization();
        }
    }
}