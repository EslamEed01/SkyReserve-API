namespace SkyReserve.Application.Contract.Authentication
{
    public record LoginRequest
     (
         string Email,
         string Password
     );
}
