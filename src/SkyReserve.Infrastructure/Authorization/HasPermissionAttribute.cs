using Microsoft.AspNetCore.Authorization;

namespace SkyReserve.Infrastructure.Authorization
{
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission) : base(policy: permission)
        {
        }
    }
}