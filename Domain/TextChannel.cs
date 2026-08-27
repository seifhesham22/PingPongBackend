namespace PingPong.API.Domain
{
    public class TextChannel : Channel
    {
        private TextChannel() { }

        public TextChannel(Guid serverId, Guid? groupId, string name, int position)
            : base(serverId, groupId, name, position)
        {
        }

        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}