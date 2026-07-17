using PingPong.API.Exceptions;

namespace PingPong.API.Domain
{
    public sealed class Friendship
    {
        public Guid Id { get; private set; }
        public Guid RequesterId { get; private set; }
        public User Requester { get; private set; } = null!;
        public Guid AddresseeId { get; private set; }
        public User Addressee { get; private set; } = null!;
        public FriendshipStatus Status { get; private set; }
        public Guid? BlockedByUserId { get; private set; }

        private Friendship() { }

        private Friendship(Guid requesterId, Guid addresseeId)
        {
            Id = Guid.NewGuid();
            RequesterId = requesterId;
            AddresseeId = addresseeId;
            Status = FriendshipStatus.Pending;
        }

        public static Friendship Request(Guid requisterId, Guid addresseeId)
        {
            if(requisterId == addresseeId)
                throw new DomainException("Cannot send a friend request to oneself.");

            return new Friendship(requisterId, addresseeId);
        }

        public bool InvolvesUser(Guid userId) => RequesterId == userId || AddresseeId == userId;

        public void Accept(Guid actorId)
        {
            if (!InvolvesUser(actorId))
                throw new DomainException("Only the requester or addressee can accept the friendship request.");

            if(actorId != AddresseeId)
                throw new DomainException("Only the addressee can accept the friendship request.");

            if (Status != FriendshipStatus.Pending)
                throw new DomainException("Friendship request is not pending.");
            Status = FriendshipStatus.Accepted;
        }
        public void Reject(Guid actorId)
        {
            if (!InvolvesUser(actorId))
                throw new DomainException("User is not part of this friendship to take action.");

            if(actorId != AddresseeId)
                throw new DomainException("Only the addressee can reject the friendship request.");

            if (Status != FriendshipStatus.Pending)
                throw new DomainException("Friendship request is not pending.");

            Status = FriendshipStatus.Rejected;
        }

        public void Block(Guid actorId)
        {
            if (!InvolvesUser(actorId))
                throw new DomainException("User is not part of this friendship to take action.");
            if (Status == FriendshipStatus.Blocked)
                throw new DomainException("Friendship is already blocked.");
            Status = FriendshipStatus.Blocked;
            BlockedByUserId = actorId;
        }

        public void Unblock(Guid actorId)
        {
            if(Status != FriendshipStatus.Blocked)
                throw new DomainException("Friendship is not blocked.");

            if(actorId != BlockedByUserId)
                throw new DomainException("Only the user who blocked the friendship can unblock it.");

            Status = FriendshipStatus.Pending;
            BlockedByUserId = null;
        }
    }
}