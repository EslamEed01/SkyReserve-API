namespace SkyReserve.Application.Consts
{
    public static class DefaultRoles
    {
        public const string superAdmin = nameof(superAdmin);
        public const string SuperAdminRoleId = "01d6368f-32c2-4d1f-867c-2f4d8d8efd3e";
        public const string SuperAdminRoleConcurrencyStamp = "a19b9133-6c62-46d0-adb2-8bcd15b27b20";

        public const string admin = nameof(admin);
        public const string AdminRoleId = "02d6368f-32c2-4d1f-867c-2f4d8d8efd3e";
        public const string AdminRoleConcurrencyStamp = "b19b9133-6c62-46d0-adb2-8bcd15b27b20";

        public const string passenger = nameof(passenger);
        public const string PassengerRoleId = "03d6368f-32c2-4d1f-867c-2f4d8d8efd3e";
        public const string PassengerRoleConcurrencyStamp = "c19b9133-6c62-46d0-adb2-8bcd15b27b20";
    }
}
