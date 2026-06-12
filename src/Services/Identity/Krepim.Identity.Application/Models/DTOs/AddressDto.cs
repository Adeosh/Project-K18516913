namespace Krepim.Identity.Application.Models.DTOs
{
    public record AddressDto(
        string? FullAddress,
        double? Latitude,
        double? Longitude,
        string? Flat
    );
}
