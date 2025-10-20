namespace SkyReserve.Application.Contract.Users
{
    public record ConfirmEmailRequest(
    string UserId,
    string Code
);
}
