namespace PingPong.API.Domain
{
    public abstract class Channel
    {
        public Guid Id { get; private set;  }
        public Guid ServerId { get; private set; }
        public Server Server { get; private set; } = null!;
        public Guid? GroupId { get; private set; }
        public ChannelGroup? Group { get; private set; }
        public string Name { get; private set; } = null!;
        public int Position { get; private set; } = 0;

        protected Channel() { }

        protected Channel(Guid serverId, Guid? groupId, string name, int position)
        {
            Id = Guid.NewGuid();
            ServerId = serverId;
            GroupId = groupId;
            Name = name;
            Position = position;
        }

        public void UpdatePosition(int position) => Position = position;
    }
}