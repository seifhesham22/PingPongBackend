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
        Read = 1 << 0,
        Write = 1 << 1,
        Delete = 1 << 2,
        Admin = 1 << 3
    }
}