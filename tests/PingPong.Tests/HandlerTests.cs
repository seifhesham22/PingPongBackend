using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PingPong.API.Domain;
using PingPong.API.Features.ChatFeatures.GetMessagesRequest;
using PingPong.API.Features.FriendShipFeature.AcceptFriendShipRequest;
using PingPong.API.Features.ServerFeatures.CreateRoleRequest;

namespace PingPong.Tests
{
    public sealed class HandlerTests
    {
        [Fact]
        public async Task AcceptFriendShip_CreatesChat_AndReusesItOnReAccept()
        {
            using var db = TestDb.Create();

            var requester = await TestDb.AddUserAsync(db, "requester");
            var addressee = await TestDb.AddUserAsync(db, "addressee");

            db.Friendships.Add(Friendship.Request(requester.Id, addressee.Id));
            await db.SaveChangesAsync();

            var handler = new AcceptFriendShip.Handler(db, new FakeCurrentUser(addressee.Id, db));

            var result = await handler.Handle(new AcceptFriendShip.Command(requester.Id), default);

            Assert.True(result.IsSuccess);

            var chats = await db.Chats.Include(c => c.ChatMembers).ToListAsync();
            Assert.Single(chats);
            Assert.Equal(2, chats[0].ChatMembers.Count);

            db.Friendships.Remove(await db.Friendships.FirstAsync());
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();

            db.Friendships.Add(Friendship.Request(requester.Id, addressee.Id));
            await db.SaveChangesAsync();

            var second = await handler.Handle(new AcceptFriendShip.Command(requester.Id), default);

            Assert.True(second.IsSuccess);
            Assert.Equal(1, await db.Chats.CountAsync());
        }

        [Fact]
        public async Task CreateRole_Fails_WhenCallerLacksManageRoles()
        {
            using var db = TestDb.Create();

            var owner = await TestDb.AddUserAsync(db, "owner");
            var member = await TestDb.AddUserAsync(db, "member");

            var server = Server.Create("Test", owner.Id, null);
            server.AddMember(member.Id);

            db.Servers.Add(server);
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();

            var handler = new CreateRole.Handler(db, new FakeCurrentUser(member.Id, db));

            var result = await handler.Handle(
                new CreateRole.Command(server.Id, "Mod", Permissions.Send_Messages), default);

            Assert.False(result.IsSuccess);
            Assert.Equal(StatusCodes.Status403Forbidden, result.Error!.StatusCode);
        }

        [Fact]
        public async Task GetMessages_PagesBackwards_WithoutSkippingOrRepeating()
        {
            using var db = TestDb.Create();

            var alice = await TestDb.AddUserAsync(db, "alice");
            var bob = await TestDb.AddUserAsync(db, "bob");

            var chat = Chat.CreateDirectChat(alice.Id, bob.Id);
            for (var i = 1; i <= 25; i++)
                chat.SendMessage(alice.Id, $"message {i}");

            db.Chats.Add(chat);
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();

            var handler = new GetMessages.Handler(db, new FakeCurrentUser(alice.Id, db));

            var seen = new List<long>();
            long? before = null;

            while (true)
            {
                var result = await handler.Handle(new GetMessages.Query(chat.Id, before, 10), default);
                Assert.True(result.IsSuccess);

                var page = result.Value!;
                seen.AddRange(page.Items.Select(m => m.number));

                if (!page.HasMore)
                    break;

                before = page.NextCursor;
                Assert.NotNull(before);
            }

            Assert.Equal(25, seen.Count);
            Assert.Equal(seen.Count, seen.Distinct().Count());
            Assert.Equal(Enumerable.Range(1, 25).Reverse().Select(i => (long)i), seen);
        }

        [Fact]
        public async Task GetMessages_Fails_WhenCallerIsNotAMember()
        {
            using var db = TestDb.Create();

            var alice = await TestDb.AddUserAsync(db, "alice");
            var bob = await TestDb.AddUserAsync(db, "bob");
            var stranger = await TestDb.AddUserAsync(db, "stranger");

            var chat = Chat.CreateDirectChat(alice.Id, bob.Id);
            db.Chats.Add(chat);
            await db.SaveChangesAsync();

            var handler = new GetMessages.Handler(db, new FakeCurrentUser(stranger.Id, db));

            var result = await handler.Handle(new GetMessages.Query(chat.Id, null, 10), default);

            Assert.False(result.IsSuccess);
            Assert.Equal(StatusCodes.Status404NotFound, result.Error!.StatusCode);
        }
    }
}