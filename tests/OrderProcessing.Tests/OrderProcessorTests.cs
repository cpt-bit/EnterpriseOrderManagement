using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OrderProcessing.API.Features.Orders;
using Xunit;

namespace OrderProcessing.Tests;

public class OrderProcessorTests()
{
    private readonly IOrderRepository _repository = Substitute.For<IOrderRepository>();
    private readonly ILogger<OrderProcessor> _logger = Substitute.For<ILogger<OrderProcessor>>();

    [Fact]
    public async Task ProcessOrderAsync_WhenOrderExists_ReturnsOrderAndCallsRepository()
    {
        // Arrange
        var expectedOrder = new Order(1, "Acme Corp", 1500m);
        _repository.GetByIdAsync(1).Returns(expectedOrder);

        var sut = new OrderProcessor(_repository, _logger);

        // Act
        var result = await sut.ProcessOrderAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedOrder);
        
        // Verify repository interaction
        await _repository.Received(1).GetByIdAsync(1);
    }

    [Fact]
    public async Task ProcessOrderAsync_WhenOrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        _repository.GetByIdAsync(999).Returns((Order?)null);

        var sut = new OrderProcessor(_repository, _logger);

        // Act
        var result = await sut.ProcessOrderAsync(999);

        // Assert
        result.Should().BeNull();
        await _repository.Received(1).GetByIdAsync(999);
    }
    [Fact]
    public async Task ProcessOrderAsync_WhenRepositoryThrows_PropagatesException()
    {
        // Arrange
        _repository.GetByIdAsync(1)
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        var sut = new OrderProcessor(_repository, _logger);

        // Act
        Func<Task> act = async () => await sut.ProcessOrderAsync(1);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Database connection failed");
    }
    
    [Fact]
    public async Task ProcessOrderAsync_WhenOrderNotFound_LogsWarning()
    {
        // Arrange
        _repository.GetByIdAsync(999).Returns((Order?)null);
        var sut = new OrderProcessor(_repository, _logger);

        // Act
        await sut.ProcessOrderAsync(999);

        // Assert
        // Verify logger received a warning call containing any formatted state
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
    [Fact]
    public async Task ApplyDiscountAsync_WhenOrderExists_CalculatesDiscountAndSaves()
    {
        // Arrange
        var initialOrder = new Order(1, "Acme Corp", 1000m);
        _repository.GetByIdAsync(1).Returns(initialOrder);

        var sut = new OrderProcessor(_repository, _logger);

        // Act
        var result = await sut.ApplyDiscountAsync(1, 20m); // 20% off 1000 = €800

        // Assert
        result.Should().BeTrue();

        // Verify SaveAsync received an Order object with Amount == €800
        await _repository.Received(1).SaveAsync(Arg.Is<Order>(o => o.Amount == 800m && o.Id == 1));
    }

    [Fact]
    public async Task DeleteOrderAsync_WhenOrderExists_DeletesAndReturnsTrue()
    {
        // Arrange
        var existingOrder = new Order(1, "Acme Corp", 1000m);
        _repository.GetByIdAsync(1).Returns(existingOrder);

        var sut = new OrderProcessor(_repository, _logger);

        // Act
        var result = await sut.DeleteOrderAsync(1);

        // Assert
        result.Should().BeTrue();
        await _repository.Received(1).DeleteAsync(1);
    }

    [Fact]
    public async Task DeleteOrderAsync_WhenOrderDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _repository.GetByIdAsync(999).Returns((Order?)null);

        var sut = new OrderProcessor(_repository, _logger);

        // Act
        var result = await sut.DeleteOrderAsync(999);

        // Assert
        result.Should().BeFalse();
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<int>());
    }
}