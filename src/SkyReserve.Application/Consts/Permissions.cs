namespace SkyReserve.Infrastructure.Authorization
{
    public static class Permissions
    {
        public const string Type = "permission";

        // Flight Management
        public static class Flights
        {
            public const string View = "Permissions.Flights.View";
            public const string Create = "Permissions.Flights.Create";
            public const string Update = "Permissions.Flights.Update";
            public const string Delete = "Permissions.Flights.Delete";
            public const string ManageAll = "Permissions.Flights.ManageAll";
        }

        // Airport Management
        public static class Airports
        {
            public const string View = "Permissions.Airports.View";
            public const string Create = "Permissions.Airports.Create";
            public const string Update = "Permissions.Airports.Update";
            public const string Delete = "Permissions.Airports.Delete";
            public const string ManageAll = "Permissions.Airports.ManageAll";
        }

        // Booking Management
        public static class Bookings
        {
            public const string View = "Permissions.Bookings.View";
            public const string ViewOwn = "Permissions.Bookings.ViewOwn";
            public const string ViewAll = "Permissions.Bookings.ViewAll";
            public const string Create = "Permissions.Bookings.Create";
            public const string Update = "Permissions.Bookings.Update";
            public const string UpdateOwn = "Permissions.Bookings.UpdateOwn";
            public const string Cancel = "Permissions.Bookings.Cancel";
            public const string CancelOwn = "Permissions.Bookings.CancelOwn";
            public const string CancelAny = "Permissions.Bookings.CancelAny";
            public const string ManageAll = "Permissions.Bookings.ManageAll";
        }

        // User Management
        public static class Users
        {
            public const string View = "Permissions.Users.View";
            public const string ViewProfile = "Permissions.Users.ViewProfile";
            public const string Create = "Permissions.Users.Create";
            public const string Update = "Permissions.Users.Update";
            public const string UpdateOwn = "Permissions.Users.UpdateOwn";
            public const string Delete = "Permissions.Users.Delete";
            public const string Disable = "Permissions.Users.Disable";
            public const string Enable = "Permissions.Users.Enable";
            public const string ManageAll = "Permissions.Users.ManageAll";
        }

        // Payment Management
        public static class Payments
        {
            public const string View = "Permissions.Payments.View";
            public const string ViewOwn = "Permissions.Payments.ViewOwn";
            public const string ViewAll = "Permissions.Payments.ViewAll";
            public const string Process = "Permissions.Payments.Process";
            public const string Refund = "Permissions.Payments.Refund";
            public const string ManageAll = "Permissions.Payments.ManageAll";
        }

        // Seat Management
        public static class Seats
        {
            public const string View = "Permissions.Seats.View";
            public const string Create = "Permissions.Seats.Create";
            public const string Update = "Permissions.Seats.Update";
            public const string Delete = "Permissions.Seats.Delete";
            public const string Assign = "Permissions.Seats.Assign";
            public const string ManageAll = "Permissions.Seats.ManageAll";
        }

        // Review Management
        public static class Reviews
        {
            public const string View = "Permissions.Reviews.View";
            public const string ViewOwn = "Permissions.Reviews.ViewOwn";
            public const string ViewAll = "Permissions.Reviews.ViewAll";
            public const string Create = "Permissions.Reviews.Create";
            public const string Update = "Permissions.Reviews.Update";
            public const string UpdateOwn = "Permissions.Reviews.UpdateOwn";
            public const string Delete = "Permissions.Reviews.Delete";
            public const string DeleteOwn = "Permissions.Reviews.DeleteOwn";
            public const string DeleteAny = "Permissions.Reviews.DeleteAny";
            public const string Moderate = "Permissions.Reviews.Moderate";
        }

        // Notification Management
        public static class Notifications
        {
            public const string View = "Permissions.Notifications.View";
            public const string ViewOwn = "Permissions.Notifications.ViewOwn";
            public const string ViewAll = "Permissions.Notifications.ViewAll";
            public const string Create = "Permissions.Notifications.Create";
            public const string Send = "Permissions.Notifications.Send";
            public const string SendBulk = "Permissions.Notifications.SendBulk";
            public const string Delete = "Permissions.Notifications.Delete";
            public const string ManageAll = "Permissions.Notifications.ManageAll";
        }

        // Passenger Management
        public static class Passengers
        {
            public const string View = "Permissions.Passengers.View";
            public const string ViewOwn = "Permissions.Passengers.ViewOwn";
            public const string ViewAll = "Permissions.Passengers.ViewAll";
            public const string Create = "Permissions.Passengers.Create";
            public const string Update = "Permissions.Passengers.Update";
            public const string UpdateOwn = "Permissions.Passengers.UpdateOwn";
            public const string Delete = "Permissions.Passengers.Delete";
            public const string ManageAll = "Permissions.Passengers.ManageAll";
        }

        // System Administration
        public static class Administration
        {
            public const string ViewDashboard = "Permissions.Administration.ViewDashboard";
            public const string ViewReports = "Permissions.Administration.ViewReports";
            public const string ViewAuditLogs = "Permissions.Administration.ViewAuditLogs";
            public const string ManageRoles = "Permissions.Administration.ManageRoles";
            public const string ManagePermissions = "Permissions.Administration.ManagePermissions";
            public const string SystemConfiguration = "Permissions.Administration.SystemConfiguration";
            public const string FullAccess = "Permissions.Administration.FullAccess";
        }

        public static IEnumerable<string> GetAllPermissions()
        {
            var permissions = new List<string>();

            var permissionClasses = typeof(Permissions).GetNestedTypes();

            foreach (var permissionClass in permissionClasses)
            {
                var fields = permissionClass.GetFields()
                    .Where(f => f.IsLiteral && f.FieldType == typeof(string));

                foreach (var field in fields)
                {
                    var value = field.GetValue(null)?.ToString();
                    if (!string.IsNullOrEmpty(value) && value.StartsWith("Permissions."))
                    {
                        permissions.Add(value);
                    }
                }
            }

            return permissions.OrderBy(p => p);
        }

        public static IEnumerable<string> GetPermissionsByCategory(string category)
        {
            return GetAllPermissions().Where(p => p.StartsWith($"Permissions.{category}."));
        }

        public static bool IsValidPermission(string permission)
        {
            return GetAllPermissions().Contains(permission);
        }
    }
}