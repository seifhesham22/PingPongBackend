using PingPong.API.Features.ChatFeatures.Hubs;
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

            SendMessage.MapEndpoint(chatGroup);
            app.MapHub<DirectChatHub>("/hubs/chat");
        }
    }
}