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
        private Friendship() { }
        public static Friendship Create(Guid requesterId, Guid addresseeId)
        {
            return new Friendship
            {
                Id = Guid.NewGuid(),
                RequesterId = requesterId,
                AddresseeId = addresseeId,
                Status = FriendshipStatus.Pending
            };
        }
        public void Accept()
        {
            if (Status != FriendshipStatus.Pending)
            {
                throw new InvalidOperationException("Friendship request is not pending.");
            }
            Status = FriendshipStatus.Accepted;
        }
        public void Reject()
        {
            if (Status != FriendshipStatus.Pending)
            {
                throw new InvalidOperationException("Friendship request is not pending.");
            }
            Status = FriendshipStatus.Rejected;
        }
    }
}