using PingPong.API.Exceptions;

namespace PingPong.API.Domain
{
    public class Role
    {
        public const int EVERY_ONE_POSITION = 0;
        public const int MAX_NAME_LENGTH = 100;
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
                Id = Guid.NewGuid(),
                ServerId = serverId,
                Name = EVERY_ONE_ROLE_NAME,
                CanJoinVoice = true,
                IsEveryone = true,
                Permissions = EVERY_ONE_PERMISSIONS,
                Position = EVERY_ONE_POSITION,
            };
        }

        internal void Update(string? name, Permissions? permissions)
        {
            if (name is not null)
            {
                if (IsEveryone)
                    throw new DomainException("The everyone role can't be renamed.");

                if (string.IsNullOrWhiteSpace(name))
                    throw new DomainException("A role must have a name.");

                if (name.Length > MAX_NAME_LENGTH)
                    throw new DomainException($"A role name can't exceed {MAX_NAME_LENGTH} characters.");

                Name = name.Trim();
            }

            if (permissions is not null)
            {
                Permissions = permissions.Value;
                CanJoinVoice = permissions.Value.HasFlag(Permissions.Connect_To_Voice);
            }
        }

        internal void MoveUp()
        {
            if (IsEveryone)
                throw new DomainException("The everyone role is always the lowest rank.");

            Position++;
        }

        internal static Role Create(Guid serverId, string name, Permissions permissions, int position)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("A role must have a name.");

            if (name.Length > MAX_NAME_LENGTH)
                throw new DomainException($"A role name can't exceed {MAX_NAME_LENGTH} characters.");

            if (position <= EVERY_ONE_POSITION)
                throw new DomainException("A custom role must rank above everyone.");

            return new Role
            {
                Id = Guid.NewGuid(),
                ServerId = serverId,
                Name = name.Trim(),
                Permissions = permissions,
                Position = position,
                IsEveryone = false,
                CanJoinVoice = permissions.HasFlag(Permissions.Connect_To_Voice),
            };
        }
    }
}