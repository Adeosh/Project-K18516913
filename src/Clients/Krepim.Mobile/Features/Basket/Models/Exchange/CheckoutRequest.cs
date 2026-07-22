namespace Krepim.Mobile.Features.Basket.Models.Exchange
{
    public record CheckoutRequest(
        string CustomerEmail,
        string? CustomerPhone,
        string FullAddress,
        double Latitude,
        double Longitude,
        string? Flat
    );
}
