﻿using System.Security.Claims;

namespace SkyReserve.Application.Services
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)
                           ?? user.FindFirst("sub")
                           ?? throw new UnauthorizedAccessException("User ID claim not found.");

            return userIdClaim.Value;
        }
    }
}
