using PingPong.API.Exceptions;

namespace PingPong.API.Domain
{
    public class ChannelGroup
    {
        public Guid Id { get; private set; }
        public Guid ServerId { get; private set; }
        public Server Server { get; private set; } = null!;
        private readonly List<Channel> _Channels = new();
        public IReadOnlyCollection<Channel> Channels => _Channels.AsReadOnly();
        public string Name { get; private set; } = null!;
        public int Position { get; private set; } = 0;

        private ChannelGroup() { }

        public ChannelGroup(Guid serverId, string name)
        {
            Id = Guid.NewGuid();
            ServerId = serverId;
            Name = name;
        }

        public void UpdateName(string name)
        {
            Name = name;
        }

        public void UpdatePosition(int position)
        { 
            Position = position;
        }

        public void AddChannel(Channel channel)
        {
            if (_Channels.Contains(channel))
                throw new DomainException("Channel already exists in this group.");

            _Channels.Add(channel);
        }

        public void RemoveChannel(Channel channel)
        {
            if (!_Channels.Contains(channel))
                throw new DomainException("Channel does not exist in this group.");

            _Channels.Remove(channel);
        }
    }
}