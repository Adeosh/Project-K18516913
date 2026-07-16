using Krepim.Ordering.Domain.Interfaces;
using Krepim.SharedKernel.Domain.Abstractions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Krepim.Ordering.Application.Tests
{
    public abstract class OrderingApplicationTestBase
    {
        protected readonly Mock<IOrderRepository> OrderRepositoryMock;
        protected readonly Mock<IUnitOfWork> UnitOfWorkMock;
        protected readonly IConfiguration Configuration;

        protected OrderingApplicationTestBase()
        {
            OrderRepositoryMock = new Mock<IOrderRepository>();
            UnitOfWorkMock = new Mock<IUnitOfWork>();

            var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ClientApp:FrontendUrl"] = "https://test-frontend.com"
            });

            Configuration = configurationBuilder.Build();
        }
    }
}
