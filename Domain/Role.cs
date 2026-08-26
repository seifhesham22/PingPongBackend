namespace PingPong.API.Domain
{
    public class Role
    {
        public const int EVERY_ONE_POSITION = 0;
        public const string EVERY_ONE_ROLE_NAME = "everyone";
        public const Permissions EVERY_ONE_PERMISSIONS =
            Permissions.Connect_To_Voice |
            Permissions.Create_Invite |
            Permissions.Send_Voice_Messages |
            Permissions.View_Channels |
            Permissions.Speak_In_Voice |
            Permissions.Send_Messages;

        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Permissions Permissions { get; set; }
        public bool CanJoinVoice { get; set; }
        public bool IsEveryone { get; set; }
        public int Position { get; set; }
        public Guid ServerId { get; set; }
        public List<UserServer> Members = new List<UserServer>();

        internal static Role CreateEveryOne(Guid serverId)
        {
            return new Role
            {
                ServerId = serverId,
                Name = EVERY_ONE_ROLE_NAME,
                CanJoinVoice = true,
                IsEveryone = true,
                Permissions = EVERY_ONE_PERMISSIONS,
                Position = EVERY_ONE_POSITION,
            };
        }
    }
}