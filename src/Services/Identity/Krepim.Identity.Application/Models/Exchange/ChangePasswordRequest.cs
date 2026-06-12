namespace Krepim.Identity.Application.Models.Exchange
{
    public record ChangePasswordRequest(string OldPassword, string NewPassword);
}
