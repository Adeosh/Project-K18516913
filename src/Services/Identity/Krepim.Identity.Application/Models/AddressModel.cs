namespace Krepim.Identity.Application.Models
{
    public record AddressModel(
        string? FullAddress,
        double? Latitude,
        double? Longitude,
        string? Flat
    );
}
