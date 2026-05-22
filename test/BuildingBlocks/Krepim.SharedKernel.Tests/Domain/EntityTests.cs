using FluentAssertions;
using Krepim.SharedKernel.Domain;

namespace Krepim.SharedKernel.Tests.Domain
{
    public class EntityTests
    {
        private sealed class TestEntity : AggregateRoot<Guid>
        {
            public string Name { get; set; }

            public TestEntity(Guid id, string name) : base(id)
            {
                Name = name;
            }
        }

        [Fact]
        public void Equals_Should_ReturnTrue_WhenIdsAreSame()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity1 = new TestEntity(id, "Name 1");
            var entity2 = new TestEntity(id, "Name 2");

            // Act & Assert
            entity1.Equals(entity2).Should().BeTrue();
            (entity1 == entity2).Should().BeTrue();
        }

        [Fact]
        public void Equals_Should_ReturnFalse_WhenIdsAreDifferent()
        {
            // Arrange
            var entity1 = new TestEntity(Guid.NewGuid(), "Same Name");
            var entity2 = new TestEntity(Guid.NewGuid(), "Same Name");

            // Act & Assert
            entity1.Equals(entity2).Should().BeFalse();
            (entity1 == entity2).Should().BeFalse();
            (entity1 != entity2).Should().BeTrue();
        }

        [Fact]
        public void Equals_Should_ReturnFalse_WhenComparedToNull()
        {
            // Arrange
            TestEntity? entity = new TestEntity(Guid.NewGuid(), "Name");
            TestEntity? nullEntity = null;

            // Act & Assert
            entity.Equals(nullEntity).Should().BeFalse();
            (entity == nullEntity).Should().BeFalse();
            (nullEntity == entity).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_Should_BeSame_ForSameId()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity1 = new TestEntity(id, "Name 1");
            var entity2 = new TestEntity(id, "Name 2");

            // Act & Assert
            entity1.GetHashCode().Should().Be(entity2.GetHashCode());
        }
    }
}
