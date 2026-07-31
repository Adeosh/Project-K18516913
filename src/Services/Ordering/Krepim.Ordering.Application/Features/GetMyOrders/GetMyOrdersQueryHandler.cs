using Krepim.Ordering.Application.Models.DTOs;
using Krepim.Ordering.Domain.Entities;
using Krepim.Ordering.Domain.Interfaces;
using Krepim.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Configuration;
using Net.Codecrete.QrCodeGenerator;

namespace Krepim.Ordering.Application.Features.GetMyOrders
{
    internal sealed class GetMyOrdersQueryHandler(
        IOrderRepository repository,
        IConfiguration configuration) : IRequestHandler<GetMyOrdersQuery, Result<List<OrderDto>>>
    {
        public async Task<Result<List<OrderDto>>> Handle(GetMyOrdersQuery request, CancellationToken ct)
        {
            List<Order> orders = await repository.GetByUserIdAsync(request.UserId, ct);
            List<OrderDto> dtos = orders.Select(o =>
            {
                string qrDataUri = GetQrCode(o);

                return new OrderDto(
                    o.Id,
                    o.CustomerEmail,
                    o.CustomerPhone,
                    o.Status.ToString(),
                    o.TotalPrice,
                    o.ShippingAddress.FullAddress,
                    o.ShippingAddress.Latitude,
                    o.ShippingAddress.Longitude,
                    o.ShippingAddress.Flat,
                    o.CreatedAt,
                    o.Items.Select(i => new OrderItemDto(i.ProductId, i.UnitPrice, i.Quantity)).ToList(),
                    qrDataUri
                );
            }).ToList();

            return dtos;
        }

        private string GetQrCode(Domain.Entities.Order o)
        {
            string baseUrl = configuration["ClientApp:FrontendUrl"] ?? "https://0.0.0.0:5173";

            string orderUrl = $"{baseUrl}/order/{o.Id}";
            var qr = QrCode.EncodeText(orderUrl, QrCode.Ecc.Medium);
            string svgContent = qr.ToSvgString(4);
            string qrDataUri = $"data:image/svg+xml;utf8,{Uri.EscapeDataString(svgContent)}";

            return qrDataUri;
        }
    }
}
