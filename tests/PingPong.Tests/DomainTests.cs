using PingPong.API.Domain;
using PingPong.API.Exceptions;
using PingPong.API.Features.Shared;

namespace PingPong.Tests
{
    public class DomainTests
    {
        [Fact]
        public void FriendShip_Resquest_Throw_WhenRequesterIsAddresse()
        {
            var userId = Guid.NewGuid();

            var act = () => Friendship.Request(userId, userId);
            Assert.Throws<DomainException>(act);
        }
        [Fact]
        public void FriendShipAccept_Throws_WhenActorIsNotAddress()
        {
            var requester = Guid.NewGuid();
            var addressee = Guid.NewGuid();
            var stranger = Guid.NewGuid();

            var friendship = Friendship.Request(requester, addressee);

            Assert.Throws<DomainException>(() => friendship.Accept(requester));
            Assert.Throws<DomainException>(() => friendship.Accept(stranger));
            Assert.Equal(FriendshipStatus.Pending, friendship.Status);

            friendship.Accept(addressee);
            Assert.Equal(FriendshipStatus.Accepted, friendship.Status);

            Assert.Throws<DomainException>(() => friendship.Accept(addressee));
        }

        [Fact]
        public void BlockThrows_WhenBlockerIsTobeBlocked()
        {
            var blocker = Guid.NewGuid();

            Assert.Throws<DomainException>(() => new Block(blocker, blocker));
        }

        [Fact]
        public void ChatThrows_WhenDmYourSelf()
        {
            var userId = Guid.NewGuid();

            Assert.Throws<DomainException>(() => Chat.CreateDirectChat(userId, userId));
        }

        [Fact]
        public void ChatThrows_WhenSenderIsNotInChat()
        {
            var userA = Guid.NewGuid();
            var userB = Guid.NewGuid();
            var stranger = Guid.NewGuid();
            string messageContent = "I am Stupid";

            var chat = Chat.CreateDirectChat(userA, userB);

            Assert.Throws<DomainException>(() => chat.SendMessage(stranger, messageContent));
        }

        [Fact]
        public void ServerThrows_WhenAddAnExistingMember()
        {
            var server = CreateServer();
            var member1 = Guid.NewGuid();
            
            server.AddMember(member1);
            Assert.Throws<DomainException>(() => server.AddMember(member1));
        }

        [Fact]
        public void Request_OrdersPairIdentically_RegardlessOfArgumentOrder()
        {
            var a = Guid.NewGuid();
            var b = Guid.NewGuid();

            var forward = Friendship.Request(a, b);
            var backward = Friendship.Request(b, a);

            Assert.Equal(forward.FirstUserId, backward.FirstUserId);
            Assert.Equal(forward.SecondUserId, backward.SecondUserId);

            Assert.Equal(a, forward.RequesterId);
            Assert.Equal(b, forward.AddresseeId);

            Assert.Equal(b, backward.RequesterId);
            Assert.Equal(a, backward.AddresseeId);
        }

        [Fact]
        public void ServerThrows_WhenAddRoleWithExistingName()
        {
            var server = CreateServer();
            var isOwner = true;
            var creatorRank = int.MaxValue;

            var permissions = Permissions.None | Permissions.Manage_Server | Permissions.Share_Screen_And_Speak;

            Assert.Throws<DomainException>(() =>
            server.CreateRole("everyone", permissions, creatorRank, isOwner));
        }

        private static Server CreateServer()
        {
            var serverOwner = Guid.NewGuid();
            var serverName = "I am stupid";
            return Server.Create(serverName, serverOwner, null);
        }

        [Fact]
        public void ServerThrows_WhenInvitorIsNoMember()
        {
            var server = CreateServer();

            Assert.Throws<DomainException>(() => server.CreateInvitation(Guid.NewGuid()));
        }

        [Fact]
        public async Task ServerThrows_WhenChannelGroupDoesntBelongToServer()
        {
            var server = CreateServer();
            var channelGroup = new ChannelGroup(Guid.NewGuid(), "Only Stupids", 1);

            Assert.Throws<DomainException>(() => server.AddChannelGroup(channelGroup));
        }
        [Fact]
        public void Chat_SendMessage_AssignsSequentialNumbers()
        {
            var alice = Guid.NewGuid();
            var bob = Guid.NewGuid();
            var chat = Chat.CreateDirectChat(alice, bob);

            var first = chat.SendMessage(alice, "one");
            var second = chat.SendMessage(bob, "two");
            var third = chat.SendMessage(alice, "three");

            Assert.Equal(1, first.Number);
            Assert.Equal(2, second.Number);
            Assert.Equal(3, third.Number);
            Assert.Equal(3, chat.LastMessageNumber);
        }

        [Fact]
        public void Chat_SendMessage_Throws_ForEmptyOrOversizedText()
        {
            var alice = Guid.NewGuid();
            var chat = Chat.CreateDirectChat(alice, Guid.NewGuid());

            Assert.Throws<DomainException>(() => chat.SendMessage(alice, ""));
            Assert.Throws<DomainException>(() => chat.SendMessage(alice, "   "));
            Assert.Throws<DomainException>(
                () => chat.SendMessage(alice, new string('x', TextMessage.MaxLength + 1)));

            Assert.Equal(0, chat.LastMessageNumber);
        }

        [Fact]
        public void Server_Create_SeedsEveryoneRole_AndGivesItToOwner()
        {
            var ownerId = Guid.NewGuid();

            var server = Server.Create("Test", ownerId, null);

            var everyone = Assert.Single(server.ServerRoles);
            Assert.True(everyone.IsEveryone);
            Assert.Equal(Role.EVERY_ONE_POSITION, everyone.Position);

            var owner = Assert.Single(server.Memberships);
            Assert.Contains(owner.Roles, r => r.IsEveryone);
        }

        [Fact]
        public void Server_AddMember_GivesTheNewMemberEveryone()
        {
            var server = Server.Create("Test", Guid.NewGuid(), null);
            var memberId = Guid.NewGuid();

            var membership = server.AddMember(memberId);

            Assert.Contains(membership.Roles, r => r.IsEveryone);
            Assert.Equal(2, server.Memberships.Count);
        }

        [Fact]
        public void Server_RemoveMember_Throws_WhenTargetIsOwner()
        {
            var ownerId = Guid.NewGuid();
            var server = Server.Create("Test", ownerId, null);

            Assert.Throws<DomainException>(() => server.RemoveMember(ownerId));
        }

        [Fact]
        public void Server_DeleteRole_Throws_ForEveryone_AndDetachesCustomRoles()
        {
            var ownerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();

            var server = Server.Create("Test", ownerId, null);
            server.AddMember(memberId);

            var everyone = server.ServerRoles.First(r => r.IsEveryone);
            Assert.Throws<DomainException>(() => server.DeleteRole(everyone.Id));

            var mod = server.CreateRole("Mod", Permissions.Kick_Members, 0, isOwner: true);
            server.AssignRole(memberId, mod.Id);

            var member = server.Memberships.First(m => m.UserId == memberId);
            Assert.Contains(member.Roles, r => r.Id == mod.Id);

            server.DeleteRole(mod.Id);

            Assert.DoesNotContain(server.ServerRoles, r => r.Id == mod.Id);
            Assert.DoesNotContain(member.Roles, r => r.Id == mod.Id);
        }

        [Fact]
        public void Server_RemoveRole_Throws_WhenRemovingEveryone()
        {
            var memberId = Guid.NewGuid();
            var server = Server.Create("Test", Guid.NewGuid(), null);
            server.AddMember(memberId);

            var everyone = server.ServerRoles.First(r => r.IsEveryone);

            Assert.Throws<DomainException>(() => server.RemoveRole(memberId, everyone.Id));
        }

        [Fact]
        public void UserServer_AllPermissions_UnionsRoles_AndHighestPositionReturnsMax()
        {
            var ownerId = Guid.NewGuid();
            var server = Server.Create("Test", ownerId, null);

            var speaker = server.CreateRole("Speaker", Permissions.Speak_In_Voice, 0, isOwner: true);
            var kicker = server.CreateRole("Kicker", Permissions.Kick_Members, 0, isOwner: true);

            server.AssignRole(ownerId, speaker.Id);
            server.AssignRole(ownerId, kicker.Id);

            var owner = server.Memberships.First(m => m.UserId == ownerId);

            Assert.True(owner.AllPermissions.HasFlag(Permissions.Speak_In_Voice));
            Assert.True(owner.AllPermissions.HasFlag(Permissions.Kick_Members));
            Assert.Equal(Math.Max(speaker.Position, kicker.Position), owner.HighestPosition);
        }

        [Fact]
        public void UserServer_AddToRole_Throws_ForCrossServerRole_AndIsIdempotent()
        {
            var memberId = Guid.NewGuid();

            var server = Server.Create("Test", Guid.NewGuid(), null);
            server.AddMember(memberId);
            var membership = server.Memberships.First(m => m.UserId == memberId);

            var other = Server.Create("Other", Guid.NewGuid(), null);
            var foreignRole = other.ServerRoles.First();

            Assert.Throws<DomainException>(() => membership.AddToRole(foreignRole));

            var mod = server.CreateRole("Mod", Permissions.Kick_Members, 0, isOwner: true);
            membership.AddToRole(mod);
            membership.AddToRole(mod);

            Assert.Equal(1, membership.Roles.Count(r => r.Id == mod.Id));
        }
    }
}