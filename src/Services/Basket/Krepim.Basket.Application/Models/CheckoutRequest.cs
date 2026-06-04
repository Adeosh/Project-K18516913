namespace Krepim.Basket.Application.Models
{
    public record CheckoutRequest(
        string FullAddress,
        double Latitude,
        double Longitude,
        string Flat
    );
}
