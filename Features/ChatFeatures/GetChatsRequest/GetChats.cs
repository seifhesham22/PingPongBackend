using MediatR;
using Microsoft.EntityFrameworkCore;
using PingPong.API.Data;
using PingPong.API.Features.Shared;

namespace PingPong.API.Features.ChatFeatures.GetChatsRequest
{
    public sealed class GetChats
    {
        public const int MaxChats = 200;
        public sealed record ChatDto(
            Guid id,
            Guid memberId,
            string memberName,
            Guid? avatarUrl,
            DateTime lastMessageAt);

        public sealed record Query : IRequest<Result<List<ChatDto>>>;

        public sealed class Handler(PingPongDbContext _db, ICurrentUser _currentUser)
            : IRequestHandler<Query, Result<List<ChatDto>>>
        {
            public async Task<Result<List<ChatDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var currentUserId = _currentUser.UserId;

                var chats = await _db.ChatMembers
                    .AsNoTracking()
                    .Where(membership => membership.UserId == currentUserId)
                    .OrderByDescending(membership => membership.Chat.LastMessageAt)
                    .Select(membership => new ChatDto(
                        id: membership.ChatId,
                        memberId: membership.Chat.ChatMembers
                            .Where(other => other.UserId != currentUserId)
                            .Select(other => other.UserId)
                            .FirstOrDefault(),
                        memberName: membership.Chat.ChatMembers
                            .Where(other => other.UserId != currentUserId)
                            .Select(other => other.User.UserName!)
                            .FirstOrDefault()!,
                        avatarUrl: membership.Chat.ChatMembers
                            .Where(other => other.UserId != currentUserId)
                            .Select(other => other.User.AvaterFileId)
                            .FirstOrDefault(),
                        lastMessageAt: membership.Chat.LastMessageAt))
                    .Take(MaxChats)
                    .ToListAsync(cancellationToken);

                return Result<List<ChatDto>>.Success(chats);
            }
        }

        public static void MapEndpoint(RouteGroupBuilder group)
        {
            group.MapGet("/my", async (
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new Query(), cancellationToken);

                return result.Match(
                    value => Results.Ok(value),
                    error => Results.Problem(
                        title: error.Message,
                        type: error.Code,
                        statusCode: error.StatusCode));
            })
            .WithName("GetMyChats");
        }
    }
}