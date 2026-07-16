namespace Krepim.Basket.Application.Models.Exchange
{
    public record CheckoutRequest(
        string CustomerEmail,
        string? CustomerPhone,
        string FullAddress,
        double Latitude,
        double Longitude,
        string Flat
    );
}
