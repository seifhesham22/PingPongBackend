using MediatR;
using Microsoft.EntityFrameworkCore;
using PingPong.API.Data;
using PingPong.API.Domain;
using PingPong.API.Features.Shared;

namespace PingPong.API.Features.ChatFeatures.GetMessagesRequest
{
    public sealed class GetMessages
    {
        public const int DefaultLimit = 50;
        public const int MaxLimit = 100;

        public sealed record MessageDto(
            Guid id,
            Guid authorId,
            string? text,
            long number,
            DateTime createdAt,
            DateTime? editedAt,
            bool isDeleted);

        public sealed record MessagePage(
            List<MessageDto> Items,
            long? NextCursor,
            bool HasMore);

        public sealed record Query(Guid ChatId, long? Before, int? Limit)
            : IRequest<Result<MessagePage>>;

        public sealed class Handler(PingPongDbContext _db, ICurrentUser _currentUser)
            : IRequestHandler<Query, Result<MessagePage>>
        {
            public async Task<Result<MessagePage>> Handle(Query request, CancellationToken cancellationToken)
            {
                var userId = _currentUser.UserId;

                var isMember = await _db.ChatMembers.AnyAsync(
                    m => m.ChatId == request.ChatId && m.UserId == userId, cancellationToken);

                if (!isMember)
                    return new Error(
                        "Chat.NotFound",
                        "Chat not found",
                        StatusCodes.Status404NotFound);

                var limit = Math.Clamp(request.Limit ?? DefaultLimit, 1, MaxLimit);

                var query = _db.Messages
                    .AsNoTracking()
                    .Where(m => m.ChatId == request.ChatId);

                if (request.Before is not null)
                    query = query.Where(m => m.Number < request.Before.Value);

                var rows = await query
                    .OrderByDescending(m => m.Number)
                    .Take(limit + 1)
                    .Select(m => new MessageDto(
                        id: m.Id,
                        authorId: m.AuthorId,
                        text: m.DeletedAt == null ? ((TextMessage)m).Text : null,
                        number: m.Number,
                        createdAt: m.CreatedAt,
                        editedAt: m.DeletedAt == null ? ((TextMessage)m).EditedAt : null,
                        isDeleted: m.DeletedAt != null))
                    .ToListAsync(cancellationToken);

                var hasMore = rows.Count > limit;
                if (hasMore)
                    rows.RemoveAt(rows.Count - 1);

                var nextCursor = hasMore && rows.Count > 0 ? rows[^1].number : (long?)null;

                return Result<MessagePage>.Success(new MessagePage(rows, nextCursor, hasMore));
            }
        }

        public static void MapEndpoint(RouteGroupBuilder group)
        {
            group.MapGet("/{chatId:guid}/messages", async (
                Guid chatId,
                long? before,
                int? limit,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new Query(chatId, before, limit), cancellationToken);

                return result.Match(
                    value => Results.Ok(value),
                    error => Results.Problem(
                        title: error.Message,
                        type: error.Code,
                        statusCode: error.StatusCode));
            })
            .WithName("GetChatMessages");
        }
    }
}