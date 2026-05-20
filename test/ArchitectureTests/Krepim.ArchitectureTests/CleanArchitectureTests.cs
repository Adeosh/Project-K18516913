using FluentAssertions;
using Krepim.Catalog.Application.Features.CreateProduct;
using Krepim.Catalog.Domain.Aggregates;
using Krepim.Identity.Application.Features.Registration;
using Krepim.Identity.Domain.Aggregates;
using NetArchTest.Rules;
using System.Reflection;

namespace Krepim.ArchitectureTests
{
    public class CleanArchitectureTests
    {
        private static readonly Assembly[] DomainAssemblies =
        [
            typeof(User).Assembly,
            typeof(Product).Assembly
        ];

        private static readonly Assembly[] ApplicationAssemblies =
        [
            typeof(RegisterCommand).Assembly,
            typeof(CreateProductCommand).Assembly
        ];

        [Fact]
        public void Domain_Should_NotHaveDependencyOn_OtherLayers()
        {
            foreach (var assembly in DomainAssemblies)
            {
                var result = Types
                    .InAssembly(assembly)
                    .ShouldNot()
                    .HaveDependencyOnAny("Krepim.Identity.Application", "Krepim.Identity.Infrastructure", "Krepim.Identity.Api",
                                         "Krepim.Catalog.Application", "Krepim.Catalog.Infrastructure", "Krepim.Catalog.Api")
                    .GetResult();

                result.IsSuccessful.Should().BeTrue($"Сборка {assembly.GetName().Name} нарушает Clean Architecture: зависит от внешних слоев. Нарушители: {string.Join(", ", result.FailingTypeNames ?? [])}");
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
                    .HaveDependencyOnAny("Krepim.Identity.Infrastructure", "Krepim.Identity.Api",
                                         "Krepim.Catalog.Infrastructure", "Krepim.Catalog.Api")
                    .GetResult();

                result.IsSuccessful.Should().BeTrue($"Сборка {assembly.GetName().Name} нарушает Clean Architecture: зависит от Infrastructure или API. Нарушители: {string.Join(", ", result.FailingTypeNames ?? [])}");
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
                    .HaveDependencyOnAny("Krepim.Identity.Domain", "Krepim.Catalog.Domain")
                    .GetResult();

                result.IsSuccessful.Should().BeTrue($"В сборке {assembly.GetName().Name} найдены Command-хэндлеры без связи с Доменом. Нарушители: {string.Join(", ", result.FailingTypeNames ?? [])}");
            }
        }
    }
}
