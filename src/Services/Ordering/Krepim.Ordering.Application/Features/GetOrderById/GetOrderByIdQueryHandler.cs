using Krepim.Ordering.Application.Models.DTOs;
using Krepim.Ordering.Domain.Interfaces;
using Krepim.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Configuration;
using Net.Codecrete.QrCodeGenerator;

namespace Krepim.Ordering.Application.Features.GetOrderById
{
    internal sealed class GetOrderByIdQueryHandler(
        IOrderRepository repository, 
        IConfiguration configuration) : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
    {
        public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken ct)
        {
            var order = await repository.GetByIdAsync(request.OrderId, ct);
            if (order is null)
                return Result<OrderDto>.Failure(new Error("Order.NotFound", "Not found", ErrorType.NotFound));

            string qrDataUri = GetQrCode(order);

            var dto = new OrderDto(
                order.Id,
                order.Status.ToString(),
                order.TotalPrice,
                order.ShippingAddress.FullAddress,
                order.ShippingAddress.Latitude,
                order.ShippingAddress.Longitude,
                order.ShippingAddress.Flat,
                order.CreatedAt,
                order.Items.Select(i => new OrderItemDto(i.ProductId, i.UnitPrice, i.Quantity)).ToList(),
                qrDataUri
            );

            return dto;
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
