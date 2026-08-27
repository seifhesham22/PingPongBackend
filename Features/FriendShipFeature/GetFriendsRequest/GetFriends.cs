using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using PingPong.API.Data;
using PingPong.API.Domain;
using PingPong.API.Features.Shared;
using PingPong.API.Features.Shared.Pagination;

namespace PingPong.API.Features.FriendShipFeature.GetFriendsRequest
{
    public sealed class GetFriends
    {
        public sealed record Query : PagedRequest, IRequest<Result<PagedResult<FriendDto>>>
        {
            public string? Search { get; init; }
        }
        public sealed record FriendDto(Guid id, string userName, Guid? avatarFileId);

        public sealed class Handler(PingPongDbContext _db, ICurrentUser _currentUser)
            : IRequestHandler<Query, Result<PagedResult<FriendDto>>>
        {
            public async Task<Result<PagedResult<FriendDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var userId = _currentUser.UserId;

                var userExists = await _currentUser.UserExistsAsync(userId);
                if (!userExists)
                    return new Error(
                        "Friendship.RequesterNotFound",
                        "Couldn't find current user.",
                        StatusCodes.Status404NotFound);

                var query = _db.Friendships
                    .AsNoTracking()
                    .Where(x => x.Status == FriendshipStatus.Accepted)
                    .Where(x => x.FirstUserId == userId || x.SecondUserId == userId)
                    .Select(x => x.FirstUserId == userId ? x.SecondUser : x.FirstUser);

                if(!string.IsNullOrWhiteSpace(request.Search))
                {
                    string pattern = $"%{request.Search.Trim()}%";
                    query = query.Where(u => EF.Functions.ILike(u.UserName!, pattern));
                }

                var totalFriendsCount = await query.CountAsync(cancellationToken);

                var friends = await query
                    .OrderBy(u => u.UserName)
                    .Skip(request.Skip)
                    .Take(request.PageSize)
                    .Select(x => new FriendDto(
                        id: x.Id,
                        userName: x.UserName!,
                        avatarFileId: x.AvaterFileId))
                    .ToListAsync(cancellationToken);

                return Result<PagedResult<FriendDto>>.Success(
                    new PagedResult<FriendDto>(
                        Items: friends,
                        Total: totalFriendsCount,
                        Page: request.Page,
                        PageSize: request.PageSize));
            }
        }

        public static void MapEndpoint(RouteGroupBuilder group)
        {
            group.MapGet("/my", async ([AsParameters] Query query, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(query, cancellationToken);

                return result.Match(
                    value => Results.Ok(value),
                    error => Results.Problem(
                        type: error.Code,
                        title: error.Message,
                        statusCode: error.StatusCode));
            })
            .WithName("GetMyFriends");
        }
    }
}
