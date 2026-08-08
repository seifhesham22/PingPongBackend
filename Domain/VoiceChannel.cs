namespace PingPong.API.Domain
{
    public class VoiceChannel : Channel
    {
        private VoiceChannel() { }

        public VoiceChannel(Guid serverId, Guid? groupId, string name, int position)
            : base(serverId, groupId, name, position)
        {
            MaxCount = DefaultMaxCount;
        }

        public const int DefaultMaxCount = 20;

        public int MaxCount { get; set; }
    }
}