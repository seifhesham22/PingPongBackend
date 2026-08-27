using Microsoft.EntityFrameworkCore;
using PingPong.API.Data;
using PingPong.API.Domain;
using PingPong.API.Features.Shared;

namespace PingPong.Tests
{
    public static class TestDb
    {
        public static PingPongDbContext Create() =>
            new(new DbContextOptionsBuilder<PingPongDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        public static async Task<User> AddUserAsync(PingPongDbContext db, string name)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = name,
                NormalizedUserName = name.ToUpperInvariant(),
                Email = $"{name}@test.local",
                NormalizedEmail = $"{name}@test.local".ToUpperInvariant(),
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();
            return user;
        }
    }

    public sealed class FakeCurrentUser(Guid userId, PingPongDbContext db) : ICurrentUser
    {
        public Guid UserId { get; } = userId;

        public Task<bool> UserExistsAsync(Guid userId) =>
            db.Users.AnyAsync(u => u.Id == userId);
    }
}