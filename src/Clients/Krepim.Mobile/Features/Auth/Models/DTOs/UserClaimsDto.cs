using Krepim.Mobile.Features.Auth.Models.Enums;

namespace Krepim.Mobile.Features.Auth.Models.DTOs
{
    public class UserClaimsDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public long Exp { get; set; }
    }
}
