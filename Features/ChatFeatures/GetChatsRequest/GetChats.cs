using MediatR;
using Microsoft.EntityFrameworkCore;
using PingPong.API.Data;
using PingPong.API.Domain;
using PingPong.API.Features.Shared;

namespace PingPong.API.Features.ChatFeatures.GetChatsRequest
{
    public sealed class GetChats
    {
        public sealed record ChatDto(
            Guid id,
            Guid memberId,
            string memberName,
            Guid? avatarUrl,
            DateTime lastMessageAt);

        public sealed record ChatPage(
            List<ChatDto> Items,
            Guid? NextCursor,
            DateTime? NextCursorAt);

        public sealed record Query(Guid? cursor, DateTime? cursorAt, int limit)
            : IRequest<Result<ChatPage>>;

        public sealed class Handler(PingPongDbContext _db, ICurrentUser _currentUser)
            : IRequestHandler<Query, Result<ChatPage>>
        {
            public async Task<Result<ChatPage>> Handle(Query request, CancellationToken cancellationToken)
            {
                var limit = request.limit;
                if (request.limit <= 0 || request.limit > 100)
                {
                    limit = 20;
                }

                var userId = _currentUser.UserId;

                var query = _db.ChatMembers
                    .AsNoTracking()
                    .Where(mine => mine.UserId == userId);

                if (request.cursor is not null || request.cursorAt is not null)
                {
                    if (request.cursor is null || request.cursorAt is null)
                        return new Error(
                            "Chat.InvalidCursor",
                            "Both cursor and cursorAt are required to page.",
                            StatusCodes.Status400BadRequest);

                    var at = request.cursorAt.Value;
                    var id = request.cursor.Value;

                    query = query.Where(mine =>
                        mine.Chat.LastMessageAt < at ||
                        (mine.Chat.LastMessageAt == at && mine.ChatId < id));
                }

                var rows = await query
                    .OrderByDescending(mine => mine.Chat.LastMessageAt)
                    .ThenByDescending(mine => mine.ChatId)
                    .Take(request.limit + 1)
                    .SelectMany(mine => mine.Chat.ChatMembers
                        .Where(other => other.UserId != userId)
                        .Select(other => new ChatDto(
                            id: mine.ChatId,
                            memberId: other.UserId,
                            memberName: other.User.UserName!,
                            avatarUrl: other.User.AvaterFileId,
                            lastMessageAt: mine.Chat.LastMessageAt)))
                    .ToListAsync(cancellationToken);

                var hasMore = rows.Count > request.limit;
                if (hasMore)
                    rows.RemoveRange(request.limit, rows.Count - request.limit);

                var last = hasMore && rows.Count > 0 ? rows[^1] : null;

                return Result<ChatPage>.Success(new ChatPage(
                    Items: rows,
                    NextCursor: last?.id,
                    NextCursorAt: last?.lastMessageAt));
            }
        }

        public static void MapEndpoint(RouteGroupBuilder group)
        {
            group.MapGet("/my", async (
                Guid? cursor,
                DateTime? cursorAt,
                int? limit,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new Query(cursor, cursorAt, limit ?? 0), cancellationToken);

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