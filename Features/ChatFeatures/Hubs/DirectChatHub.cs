using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PingPong.API.Data;
using PingPong.API.Domain;

namespace PingPong.API.Features.ChatFeatures.Hubs
{
    public sealed record MessageDto(
       Guid Id,
       Guid ChatId,
       Guid AuthorId,
       string Text,
       long Number,
       DateTime CreatedAt);
    public interface IChatClient
    {
        Task ReceiveMessage(MessageDto message);
    }
    [Authorize]
    public class DirectChatHub(PingPongDbContext _db) : Hub<IChatClient>
    {
        public async Task JoinChat(Guid chatId)
        {
            var currentUser = Guid.Parse(Context.UserIdentifier!);

            var IsMember = await _db.ChatMembers.AnyAsync(x => x.ChatId == chatId && x.UserId == currentUser);
            if (!IsMember)
                throw new HubException("User is not member of this chat");

            await Groups.AddToGroupAsync(Context.ConnectionId, ChatGroup(chatId));
        }

        public async Task LeaveChat(Guid chatId) => 
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ChatGroup(chatId));

        public static string ChatGroup(Guid chatId) => $"chat:{chatId}";
    }
}
