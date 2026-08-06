using PingPong.API.Exceptions;

namespace PingPong.API.Domain
{
    public class Block
    {
        public Guid Id { get; private set; }
        public Guid BlockerId { get; private set; }
        public User Blocker { get; private set; } = null!;
        public Guid BlockedId { get; private set; }
        public User Blocked { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; }
        private Block() { }
        public Block(Guid blockerId, Guid blockedId)
        {
            if(blockerId == blockedId)
                throw new DomainException("You cannot block yourself.");
            Id = Guid.NewGuid();
            BlockerId = blockerId;
            BlockedId = blockedId;
            CreatedAt = DateTime.UtcNow;
        }
    }
}