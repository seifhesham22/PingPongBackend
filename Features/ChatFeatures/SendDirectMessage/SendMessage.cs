using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PingPong.API.Data;
using PingPong.API.Domain;
using PingPong.API.Exceptions;
using PingPong.API.Features.ChatFeatures.Hubs;
using PingPong.API.Features.Shared;

namespace PingPong.API.Features.ChatFeatures.SendDirectMessage
{
    public class SendMessage
    {
        public sealed record Body(string text);
        public sealed record Command(Guid chatId, string text) : IRequest<Result<MessageDto>>;

        public sealed class Handler(
            PingPongDbContext _db,
            ICurrentUser _currentUser,
            IHubContext<DirectChatHub, IChatClient> _hub) : IRequestHandler<Command, Result<MessageDto>>
        {
            private const int MAXATTEMPTS = 3;
            public async Task<Result<MessageDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var userId = _currentUser.UserId;

                for(int i = 1; i <=  MAXATTEMPTS; i++)
                {
                    var chat = await _db.Chats
                        .Include(x => x.ChatMembers)
                        .FirstOrDefaultAsync(c => c.Id == request.chatId, cancellationToken);

                    if (chat == null || !chat.HasMember(userId))
                        return new Error(
                            "Chat.NotFound",
                            "Chat not found",
                            StatusCodes.Status404NotFound);

                    var otherUserId = chat.ChatMembers.First(x => x.UserId != userId).UserId;

                    var allowed = await CanSendMessageAsync(userId, otherUserId, cancellationToken);
                    if (allowed is not null)
                        return allowed;

                    TextMessage message;
                    try
                    {
                        message = chat.SendMessage(userId, request.text);
                    }
                    catch(DomainException ex)
                    {
                        return new Error(
                            "Message.Invalid",
                            ex.Message,
                            StatusCodes.Status400BadRequest);
                    }

                    try
                    {
                        await _db.SaveChangesAsync(cancellationToken);
                    }
                    catch (DbUpdateConcurrencyException) when (i < MAXATTEMPTS)
                    {
                        _db.ChangeTracker.Clear();
                        continue;
                    }

                    var dto = new MessageDto(
                       message.Id, chat.Id, userId, message.Text,
                       message.Number, message.CreatedAt);

                    await _hub.Clients
                        .Group(DirectChatHub.ChatGroup(chat.Id))
                        .ReceiveMessage(dto);

                    return Result<MessageDto>.Success(dto);
                }

                return new Error(
                    "Chat.Busy",
                    "Too much activity. Try again.",
                    StatusCodes.Status409Conflict);
            }

            public async Task<Error?> CanSendMessageAsync(Guid userId, Guid otherUserId, CancellationToken ct)
            {
                var block = await _db.Blocks.AnyAsync(
                    x => (x.BlockerId == userId && x.BlockedId == otherUserId)
                    || (x.BlockerId == otherUserId && x.BlockedId == userId), ct);

                if (block)
                    return new Error(
                        "Chat.Blocked",
                        "You can't send message to this user",
                        StatusCodes.Status403Forbidden);
                return null;
            }
        }

        public static void MapEndpoint(RouteGroupBuilder group)
        {
            group.MapPost("/{chatId:guid}/messages", async (
                Guid chatId,
                Body body,
                ISender sender,
                CancellationToken ct) =>
            {
                var res = await sender.Send(new Command(chatId, body.text), ct);

                return res.Match(
                    value => Results.Ok(value),
                    error => Results.Problem(
                        title: error.Message,
                        type: error.Code,
                        statusCode: error.StatusCode));
            });
        }
    }
}