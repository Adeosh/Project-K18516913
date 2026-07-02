using FluentAssertions;
using Krepim.Catalog.Application.Features.CreateProduct;
using Krepim.Catalog.Application.Interfaces;
using Krepim.Catalog.Application.Models.DTOs;
using Krepim.Catalog.Domain.Aggregates;
using Krepim.Catalog.Domain.Enums;
using Krepim.EventBus.Events.Catalog;
using Krepim.SharedKernel.Domain.Abstractions;
using MassTransit;
using NSubstitute;

namespace Krepim.Catalog.Application.Tests.Features.Products
{
    public class CreateProductCommandHandlerTests
    {
        private readonly IProductWriteRepository _repositoryMock;
        private readonly IPublishEndpoint _publishEndpointMock;
        private readonly IUnitOfWork _unitOfWorkMock;
        private readonly CreateProductCommandHandler _handler;

        public CreateProductCommandHandlerTests()
        {
            _repositoryMock = Substitute.For<IProductWriteRepository>();
            _publishEndpointMock = Substitute.For<IPublishEndpoint>();
            _unitOfWorkMock = Substitute.For<IUnitOfWork>();

            _handler = new CreateProductCommandHandler(_repositoryMock, _publishEndpointMock, _unitOfWorkMock);
        }

        [Fact]
        public async Task Handle_Should_ReturnSuccessAndPublishEvent_WhenDataIsValid()
        {
            // Arrange
            var command = new CreateProductCommand(
                Name: "Заклепка резьбовая цилиндрическая с малым фланцем М6х15",
                Description: "Заклепка резьбовая цилиндрическая с малым фланцем",
                Sku: "ZRM-M6X15-ST-ZN",
                Price: 4.50m,
                CategoryId: Guid.Parse("4d6804ce-708d-4fd3-994f-641d65ebbae5"),
                ImageUrls: new[]
                {
                    "http://localhost:9000/catalog-images/0780d3b0-b469-44f2-9ca0-25645206f1c0-7492748982.jpeg",
                    "http://localhost:9000/catalog-images/2e3733dd-d760-483f-9970-c266b854f9c1-7492748983.jpeg"
                },
                Standard: "DIN 7338",
                SalesUnit: SalesUnit.Pcs,
                SalesStep: 250m,
                Attributes: new Dictionary<string, string>
                {
                    { "Длина (L)", "15 мм" },
                    { "Материал", "Оцинкованная сталь" },
                    { "Тип буртика", "Уменьшенный (потайной)" },
                    { "Форма корпуса", "Цилиндрическая с насечкой" },
                    { "Диаметр резьбы (d)", "М6" },
                    { "Толщина скрепляемых материалов", "1.5 - 3.0 мм" }
                },
                PriceTiers: new List<PriceTierDto>
                {
                    new(1000, 3.8m, "RUB"),
                    new(5000, 3.1m, "RUB"),
                    new(20000, 2.65m, "RUB")
                }
            );

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();

            await _repositoryMock.Received(1).AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());

            await _publishEndpointMock.Received(1).Publish(
                Arg.Is<ProductCreatedIntegrationEvent>(e => e.ProductId == result.Value),
                Arg.Any<CancellationToken>());

            await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
