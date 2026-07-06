using FluentAssertions;
using Krepim.Basket.Application.Features.Checkout;
using Krepim.Basket.Domain.Interfaces;
using Krepim.Catalog.Application.Features.CreateProduct;
using Krepim.Catalog.Domain.Aggregates;
using Krepim.Identity.Application.Features.Registration;
using Krepim.Identity.Domain.Aggregates;
using Krepim.Inventory.Application.Features.GetStock;
using Krepim.Inventory.Domain.Entities;
using Krepim.Ordering.Application.Features.GetMyOrders;
using Krepim.Ordering.Domain.Entities;
using Krepim.Payment.Application.Features.ProcessPayment;
using Krepim.Payment.Domain.Entities;
using NetArchTest.Rules;
using System.Reflection;

namespace Krepim.ArchitectureTests
{
    public class CleanArchitectureTests
    {
        private static readonly Assembly[] DomainAssemblies =
        [
            typeof(User).Assembly,
            typeof(Product).Assembly,
            typeof(IBasketRepository).Assembly,
            typeof(StockItem).Assembly,
            typeof(Order).Assembly,
            typeof(PaymentTransaction).Assembly
        ];

        private static readonly Assembly[] ApplicationAssemblies =
        [
            typeof(RegisterCommand).Assembly,
            typeof(CreateProductCommand).Assembly,
            typeof(CheckoutBasketCommand).Assembly,
            typeof(GetStockQuery).Assembly,
            typeof(GetMyOrdersQuery).Assembly,
            typeof(ProcessPaymentWebhookCommand).Assembly
        ];

        private static readonly string[] ApplicationNamespaces =
        [
            "Krepim.Identity.Application", "Krepim.Catalog.Application", "Krepim.Basket.Application",
            "Krepim.Inventory.Application", "Krepim.Ordering.Application", "Krepim.Payment.Application"
        ];

        private static readonly string[] InfrastructureAndApiNamespaces =
        [
            "Krepim.Identity.Infrastructure", "Krepim.Identity.Api",
            "Krepim.Catalog.Infrastructure", "Krepim.Catalog.Api",
            "Krepim.Basket.Infrastructure", "Krepim.Basket.Api",
            "Krepim.Inventory.Infrastructure", "Krepim.Inventory.Api",
            "Krepim.Ordering.Infrastructure", "Krepim.Ordering.Api",
            "Krepim.Payment.Infrastructure", "Krepim.Payment.Api"
        ];

        private static readonly string[] DomainNamespaces =
        [
            "Krepim.Identity.Domain", "Krepim.Catalog.Domain", "Krepim.Basket.Domain",
            "Krepim.Inventory.Domain", "Krepim.Ordering.Domain", "Krepim.Payment.Domain"
        ];

        [Fact]
        public void Domain_Should_NotHaveDependencyOn_OtherLayers()
        {
            var forbiddenNamespaces = ApplicationNamespaces.Concat(InfrastructureAndApiNamespaces).ToArray();

            foreach (var assembly in DomainAssemblies)
            {
                var result = Types
                    .InAssembly(assembly)
                    .ShouldNot()
                    .HaveDependencyOnAny(forbiddenNamespaces)
                    .GetResult();

                result.IsSuccessful.Should().BeTrue(
                    $"Сборка {assembly.GetName().Name} нарушает Clean Architecture: зависит от внешних слоев. Нарушители: {string.Join(", ", result.FailingTypeNames ?? [])}");
            }
        }

        [Fact]
        public void Application_Should_NotHaveDependencyOn_InfrastructureOrApi()
        {
            foreach (var assembly in ApplicationAssemblies)
            {
                var result = Types
                    .InAssembly(assembly)
                    .ShouldNot()
                    .HaveDependencyOnAny(InfrastructureAndApiNamespaces)
                    .GetResult();

                result.IsSuccessful.Should().BeTrue(
                    $"Сборка {assembly.GetName().Name} нарушает Clean Architecture: зависит от Infrastructure или API. Нарушители: {string.Join(", ", result.FailingTypeNames ?? [])}");
            }
        }

        [Fact]
        public void CommandHandlers_Should_HaveDependencyOnDomain()
        {
            foreach (var assembly in ApplicationAssemblies)
            {
                var result = Types
                    .InAssembly(assembly)
                    .That()
                    .HaveNameEndingWith("CommandHandler")
                    .Should()
                    .HaveDependencyOnAny(DomainNamespaces)
                    .GetResult();

                result.IsSuccessful.Should().BeTrue(
                    $"В сборке {assembly.GetName().Name} найдены Command-хэндлеры без связи с Доменом. Нарушители: {string.Join(", ", result.FailingTypeNames ?? [])}");
            }
        }
    }
}
