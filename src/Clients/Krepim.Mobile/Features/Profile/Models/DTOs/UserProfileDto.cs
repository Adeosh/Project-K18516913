namespace Krepim.Mobile.Features.Profile.Models.DTOs
{
    public class UserProfileDto
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public AddressDto? DefaultAddress { get; set; }
    }
}
