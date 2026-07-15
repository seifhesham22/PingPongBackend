using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Query.Internal;
using PingPong.API.Exceptions;

namespace PingPong.API.Domain
{
    public class User : IdentityUser
    {
        public Guid? AvaterFileId { get; private set; }

        private readonly List<UserServer> _MemberShips = new List<UserServer>();
        public IReadOnlyCollection<UserServer> Memberships => _MemberShips.AsReadOnly();

        private readonly List<Chat> _Chats = new List<Chat>();
        public IReadOnlyCollection<Chat> Chats => _Chats.AsReadOnly();

        private readonly List<UserDeviceToken> _DeviceTokens = new List<UserDeviceToken>();
        public IReadOnlyCollection<UserDeviceToken> DeviceTokens => _DeviceTokens.AsReadOnly();

        private User() { }

        public void UpdateAvatar(Guid newAvatarId)
        {
            if(AvaterFileId == null)
                throw new DomainException("User does not have an avatar to update.");

            AvaterFileId = newAvatarId;
        }
        public void AddMembership(UserServer membership)
        {
            if (_MemberShips.Any(m => m.ServerId == membership.ServerId))
                throw new DomainException("User is already a member of this server.");

            _MemberShips.Add(membership);
        }

        public void RemoveMembership(Guid serverId)
        {
            var membership = _MemberShips.FirstOrDefault(m => m.ServerId == serverId) ??
                throw new DomainException("User is not a member of this server."); ;
                
            _MemberShips.Remove(membership);
        }

        public void AddChat(Chat chat)
        {
            if (_Chats.Any(c => c.Id == chat.Id))
                throw new DomainException("User is already in this chat.");

            _Chats.Add(chat);
        }

        public void DeleteChat(Guid chatId)
        {
            var chat = _Chats.FirstOrDefault(c => c.Id == chatId) ??
                throw new DomainException("User is not in this chat.");

            _Chats.Remove(chat);
        }
    }
}