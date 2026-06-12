namespace Krepim.Basket.Application.Models.Exchange
{
    public record CheckoutRequest(
        string FullAddress,
        double Latitude,
        double Longitude,
        string Flat
    );
}
