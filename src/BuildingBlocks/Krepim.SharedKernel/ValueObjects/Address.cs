namespace Krepim.SharedKernel.ValueObjects
{
    public sealed record Address(
        string FullAddress,
        double? Latitude,
        double? Longitude,
        string? Flat
    );
}
