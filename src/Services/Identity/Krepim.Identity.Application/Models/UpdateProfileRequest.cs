namespace Krepim.Identity.Application.Models
{
    public record UpdateProfileRequest(string Email, string? PhoneNumber, AddressModel? DefaultAddress);
}
