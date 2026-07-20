namespace Krepim.Mobile.Features.Profile.Models.DTOs
{
    public class AddressDto
    {
        public string FullAddress { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Flat { get; set; }
    }
}
