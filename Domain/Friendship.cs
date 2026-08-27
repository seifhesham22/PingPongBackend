using System.ComponentModel.DataAnnotations.Schema;
using PingPong.API.Exceptions;

namespace PingPong.API.Domain
{
    public sealed class Friendship
    {
        public Guid Id { get; private set; }
        public Guid FirstUserId { get; private set; }
        public User FirstUser { get; private set; } = null!;
        public Guid SecondUserId { get; private set; }
        public User SecondUser { get; private set; } = null!;
        public Guid RequesterId { get; private set; }
        public FriendshipStatus Status { get; private set; }
        [NotMapped]
        public Guid AddresseeId => RequesterId == FirstUserId ? SecondUserId : FirstUserId;

        private Friendship() { }

        private Friendship(Guid firstUserId, Guid secondUserId, Guid requesterId)
        {
            Id = Guid.NewGuid();
            FirstUserId = firstUserId;
            SecondUserId = secondUserId;
            Status = FriendshipStatus.Pending;
            RequesterId = requesterId;
        }

        public static Friendship Request(Guid requesterId, Guid addresseeId)
        {
            if(requesterId == addresseeId)
                throw new DomainException("Cannot send a friend request to oneself.");

            var(firstUserId, secondUserId) = OrderPair(requesterId, addresseeId);

            return new Friendship(firstUserId, secondUserId, requesterId);
        }

        public static (Guid First, Guid Second) OrderPair(Guid first, Guid second) => 
            first.CompareTo(second) < 0 ? (first, second) : (second, first);

        public bool InvolvesUser(Guid userId) => FirstUserId == userId || SecondUserId == userId;

        public void Accept(Guid actorId)
        {
            if (!InvolvesUser(actorId))
                throw new DomainException("Only the requester or addressee can accept the friendship request.");

            if(actorId != AddresseeId)
                throw new DomainException("Only the addressee can accept the friendship request.");

            if(Status != FriendshipStatus.Pending)
            {
                throw new DomainException(Status switch { 
                    FriendshipStatus.Accepted => "Users are already friends.",
                    _ => "Invalid friendship status." });
            }
            Status = FriendshipStatus.Accepted;
        }
    }
}