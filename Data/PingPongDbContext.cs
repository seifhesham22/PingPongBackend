using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
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
        public DbSet<Block> Blocks => Set<Block>();
        public DbSet<FileMetaData> Files => Set<FileMetaData>();
        public PingPongDbContext(DbContextOptions<PingPongDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
                .HasIndex(x => x.NormalizedEmail)
                .HasDatabaseName("Email_Index")
                .IsUnique();

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

            builder.Entity<Chat>()
                .Property<uint>("Version")
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();

            builder.Entity<Message>()
            .HasOne(m => m.ReplyTo)
            .WithMany()
            .HasForeignKey(m => m.ReplyToId)
            .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<MessageMention>().HasIndex(m => m.UserId);
            builder.Entity<MessageMention>().HasIndex(m => m.RoleId);
            builder.Entity<MessageMention>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Mention_UserXorRole",
                    "(\"UserId\" IS NULL) <> (\"RoleId\" IS NULL)"));

            builder.Entity<UserServer>()
                .HasMany(us => us.Roles)
                .WithMany(r => r.Members)
                .UsingEntity(j => j.ToTable("UserServerRoles"));

            builder.Entity<UserServer>()
                .HasIndex(us => new { us.UserId, us.ServerId })
                .IsUnique();

            builder.Entity<User>().HasOne(x => x.FileMetaData)
                .WithOne(x => x.User).HasForeignKey<FileMetaData>(x => x.UserId);


            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var navigation in entityType.GetNavigations().Where(n => n.IsCollection))
                {
                    navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
                }
            }
            builder.Entity<Server>()
            .Navigation(x => x.Memberships)
            .HasField("_Memberships")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Entity<User>()
            .Navigation(x => x.Memberships)
            .HasField("_MemberShips")
            .UsePropertyAccessMode(PropertyAccessMode.Field);


            builder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            builder.Entity<User>().HasIndex(u => u.UserName).IsUnique();
            builder.Entity<ServerInvitation>().HasIndex(i => i.Token).IsUnique();

            builder.Entity<Friendship>()
                .HasIndex(f => new { f.FirstUserId, f.SecondUserId })
                .IsUnique();

            builder.Entity<Friendship>()
                .HasOne(f => f.FirstUser).WithMany()
                .HasForeignKey(f => f.FirstUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Friendship>()
                .HasOne(f => f.SecondUser).WithMany()
                .HasForeignKey(f => f.SecondUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Reaction>()
                .HasIndex(r => new { r.MessageId, r.UserId, r.EmojiCode })
                .IsUnique();

            builder.Entity<PollVote>()
                .HasIndex(v => new { v.PollOptionId, v.UserId })
                .IsUnique();

            builder.Entity<LastReadMessage>().HasIndex(l => new { l.UserId, l.ChannelId }).IsUnique();
            builder.Entity<LastReadMessage>().HasIndex(l => new { l.UserId, l.ChatId }).IsUnique();

            builder.Entity<UserDeviceToken>().HasIndex(t => t.Token).IsUnique();

            builder.Entity<ChatMember>().HasIndex(m => new { m.UserId, m.ChatId }).IsUnique();

            builder.Entity<Chat>().HasIndex(c => new { c.LastMessageAt, c.Id }).IsDescending();

            builder.Entity<Message>().HasIndex(m => new { m.ChannelId, m.CreatedAt });

            builder.Entity<Block>().HasIndex(m => new { m.BlockerId, m.BlockedId}).IsUnique();
            builder.Entity<Block>().HasOne(x => x.Blocker)
                .WithMany()
                .HasForeignKey(x => x.BlockerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Block>().HasOne(x => x.Blocked)
                .WithMany()
                .HasForeignKey(x => x.BlockedId)
                .OnDelete(DeleteBehavior.NoAction);

            base.OnModelCreating(builder);
        }
    }
}