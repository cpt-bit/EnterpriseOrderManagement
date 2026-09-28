using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OrderProcessing.API.Features.Orders;
using OrderProcessing.API.Data;
using Xunit;

namespace OrderProcessing.Tests;

public class EfOrderRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public EfOrderRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new AppDbContext(_options);

        context.Database.EnsureCreated();

        context.Orders.AddRange(
            new Order(1, "Acme Corp", 1500m),
            new Order(2, "Apex Ltd", 3200m)
        );

        context.SaveChanges();
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderExists_ReturnsOrder()
    {
        // Arrange
        await using var context = new AppDbContext(_options);
        var sut = new EfOrderRepository(context);

        // Act
        var result = await sut.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new Order(1, "Acme Corp", 1500m));
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        await using var context = new AppDbContext(_options);
        var sut = new EfOrderRepository(context);

        // Act
        var result = await sut.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllSeededOrders()
    {
        // Arrange
        await using var context = new AppDbContext(_options);
        var sut = new EfOrderRepository(context);

        // Act
        var result = await sut.GetAllAsync();

        // Assert
        result.Should().BeEquivalentTo(
        [
            new Order(1, "Acme Corp", 1500m),
            new Order(2, "Apex Ltd", 3200m)
        ]);
    }

    [Fact]
    public async Task SaveAsync_WhenOrderExists_UpdatesOrder()
    {
        // Arrange
        await using var context = new AppDbContext(_options);
        var sut = new EfOrderRepository(context);

        var updatedOrder = new Order(1, "Acme Corp", 999m);

        // Act
        await sut.SaveAsync(updatedOrder);

        // Assert
        var savedOrder = await context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == 1);

        savedOrder.Should().NotBeNull();
        savedOrder.Should().BeEquivalentTo(updatedOrder);
    }

    [Fact]
    public async Task DeleteAsync_WhenOrderExists_RemovesOrder()
    {
        // Arrange
        await using var context = new AppDbContext(_options);
        var sut = new EfOrderRepository(context);

        // Act
        await sut.DeleteAsync(1);

        // Assert
        var deletedOrder = await context.Orders.FindAsync(1);
        deletedOrder.Should().BeNull();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
