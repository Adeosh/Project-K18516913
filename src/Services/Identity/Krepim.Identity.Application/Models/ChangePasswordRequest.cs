namespace Krepim.Identity.Application.Models
{
    public record ChangePasswordRequest(string OldPassword, string NewPassword);
}
