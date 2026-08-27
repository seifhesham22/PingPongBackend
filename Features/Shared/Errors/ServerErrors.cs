namespace PingPong.API.Features.Shared
{
    public static class ServerErrors
    {
        public static Error NotFound => new(
            "Server.NotFound",
            "Couldn't find this server.",
            StatusCodes.Status404NotFound);

        public static Error MemberNotFound => new(
            "Server.MemberNotFound",
            "This user is not a member of the server.",
            StatusCodes.Status404NotFound);

        public static Error MemberOutranked => new(
            "Server.MemberOutranked",
            "You can only manage members ranked below your own.",
            StatusCodes.Status403Forbidden);

        public static Error RoleNotFound => new(
            "Role.NotFound",
            "Couldn't find this role on the server.",
            StatusCodes.Status404NotFound);

        public static Error CannotManageRoles => new(
            "Role.Forbidden",
            "You don't have permission to manage roles on this server.",
            StatusCodes.Status403Forbidden);

        public static Error Outranked => new(
            "Role.Outranked",
            "You can only act on roles ranked below your own.",
            StatusCodes.Status403Forbidden);

        public static Error PermissionNotHeld => new(
            "Role.PermissionNotHeld",
            "You can't grant a permission you don't have yourself.",
            StatusCodes.Status403Forbidden);

        public static Error UnknownPermission => new(
            "Role.UnknownPermission",
            "The requested permissions contain an unknown value.",
            StatusCodes.Status400BadRequest);

        public static Error OwnRole => new(
            "Role.OwnRole",
            "You can't update a role that you hold yourself.",
            StatusCodes.Status403Forbidden);
        public static Error AdminOnly => new(
            "Role.AdminOnly",
            "Only an admin can update the everyone role.",
            StatusCodes.Status403Forbidden);
    }
}