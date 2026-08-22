namespace PingPong.API.Domain
{
    public class Role
    {
        public Guid Id { get; set; }
        public Permissions Permissions { get; set; }
        public bool CanJoinVoice { get; private set; }
        public int Position { get; set; }
        public Guid ServerId { get; set; }
        public List<UserServer> Members = new List<UserServer>();
    }
    public enum Permissions
    {
        None = 0,
        View_Channels = 1 << 0,
        Manage_Channels = 1 << 1,
        Manage_Roles = 1 << 2,
        Manage_Server = 1 << 3,
        Create_Invite = 1 << 4,
        Kick_Members = 1 << 5,
        Send_Messages = 1 << 6,
        Attach_File = 1 << 7,
        Manage_Messages = 1 << 8,
        Send_Voice_Messages = 1 << 9,
        Connect_To_Voice = 1 << 10,
        Speak_In_Voice = 1 << 11,
        Share_Screen_And_Speak = 1 << 12,
        Mute_Members = 1 << 13,
        Move_Members = 1 << 14,
        Admin = 1 << 15
    }
}