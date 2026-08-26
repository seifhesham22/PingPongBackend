using PingPong.API.Features.ChatFeatures.Hubs;
using PingPong.API.Features.ChatFeatures.GetChatsRequest;
using PingPong.API.Features.ChatFeatures.SendDirectMessage;

namespace PingPong.API.Features.ChatFeatures
{
    public static class ChatEndpoints
    {
        public static void MapChatEndpoints(this WebApplication app)
        {
            var chatGroup = app.MapGroup("/chat")
                .WithTags("Chats")
                .RequireAuthorization();

            GetChats.MapEndpoint(chatGroup);
            SendMessage.MapEndpoint(chatGroup);
            app.MapHub<ChatHub>("/hubs/chat");
        }
    }
}