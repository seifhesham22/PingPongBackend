using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PingPong.API.Domain;

namespace PingPong.API.Data
{
    public class PingPongDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public DbSet<Server> Servers => Set<Server>();
        public DbSet<UserServer> UserServers => Set<UserServer>();
        public DbSet<Role> ServerRoles => Set<Role>();
        public DbSet<Channel> Channels => Set<Channel>();
        public DbSet<ChannelGroup> ChannelGroups => Set<ChannelGroup>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<Chat> Chats => Set<Chat>();
        public DbSet<ChatMember> ChatMembers => Set<ChatMember>();
        public DbSet<Friendship> Friendships => Set<Friendship>();
        public DbSet<ServerInvitation> ServerInvitations => Set<ServerInvitation>();
        public DbSet<Attachment> Attachments => Set<Attachment>();
        public DbSet<Reaction> Reactions => Set<Reaction>();
        public DbSet<MessageMention> MessageMentions => Set<MessageMention>();
        public DbSet<LastReadMessage> LastReadMessages => Set<LastReadMessage>();
        public DbSet<UserDeviceToken> UserDeviceTokens => Set<UserDeviceToken>();

        public PingPongDbContext(DbContextOptions<PingPongDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Channel>()
            .HasDiscriminator<string>("ChannelType")
            .HasValue<TextChannel>("text")
            .HasValue<VoiceChannel>("voice");

            builder.Entity<Message>()
            .HasDiscriminator<string>("MessageType")
            .HasValue<TextMessage>("text")
            .HasValue<PollMessage>("poll");

            builder.Entity<Role>()
            .Property(r => r.Permissions)
            .HasConversion<long>();

            builder.Entity<Role>()
            .HasIndex(r => new { r.ServerId, r.Position })
            .IsUnique();

            builder.Entity<Role>()
            .HasIndex(r => r.ServerId)
            .IsUnique();
            //.HasFilter("\"IsEveryone\" = true");

            builder.Entity<Message>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Message_ChannelXorChat",
                "(\"ChannelId\" IS NULL) <> (\"ChatId\" IS NULL)"));

            builder.Entity<Message>().HasIndex(m => new { m.ChannelId, m.Number }).IsUnique();
            builder.Entity<Message>().HasIndex(m => new { m.ChatId, m.Number }).IsUnique();

            builder.Entity<Message>()
            .HasOne(m => m.ReplyTo)
            .WithMany()
            .HasForeignKey(m => m.ReplyToId)
            .OnDelete(DeleteBehavior.NoAction);

            // ---- Mentions: a real join table, so "my mentions" is indexed ----
            builder.Entity<MessageMention>().HasIndex(m => m.UserId);
            builder.Entity<MessageMention>().HasIndex(m => m.RoleId);
            builder.Entity<MessageMention>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Mention_UserXorRole",
                    "(\"UserId\" IS NULL) <> (\"RoleId\" IS NULL)"));

            // ---- Many-to-many: UserServer <-> Role ----
            builder.Entity<UserServer>()
                .HasMany(us => us.Roles)
                .WithMany(r => r.Members)
                .UsingEntity(j => j.ToTable("UserServerRoles"));

            builder.Entity<UserServer>()
                .HasIndex(us => new { us.UserId, us.ServerId })
                .IsUnique();

            // ---- Uniqueness ----
            builder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            builder.Entity<User>().HasIndex(u => u.UserName).IsUnique();
            builder.Entity<ServerInvitation>().HasIndex(i => i.Token).IsUnique();

            // Friendship: one row per pair, regardless of direction.
            // Enforce ordering (FirstUserId < SecondUserId) in a DB trigger or at insert time.
            builder.Entity<Friendship>()
                .HasIndex(f => new { f.RequesterId, f.AddresseeId })
                .IsUnique();

            builder.Entity<Friendship>()
                .HasOne(f => f.Requester).WithMany()
                .HasForeignKey(f => f.RequesterId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Friendship>()
                .HasOne(f => f.Addressee).WithMany()
                .HasForeignKey(f => f.AddresseeId)
                .OnDelete(DeleteBehavior.NoAction);

            // ---- Reactions: one per user per emoji per message ----
            builder.Entity<Reaction>()
                .HasIndex(r => new { r.MessageId, r.UserId, r.EmojiCode })
                .IsUnique();

            // ---- Poll votes: one per user per option ----
            builder.Entity<PollVote>()
                .HasIndex(v => new { v.PollOptionId, v.UserId })
                .IsUnique();

            // ---- Unread tracking ----
            builder.Entity<LastReadMessage>().HasIndex(l => new { l.UserId, l.ChannelId }).IsUnique();
            builder.Entity<LastReadMessage>().HasIndex(l => new { l.UserId, l.ChatId }).IsUnique();

            builder.Entity<UserDeviceToken>().HasIndex(t => t.Token).IsUnique();

            // ---- Read path: fetching a channel's recent messages ----
            builder.Entity<Message>().HasIndex(m => new { m.ChannelId, m.CreatedAt });

            base.OnModelCreating(builder);
        }
    }
}