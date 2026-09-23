namespace OrderProcessing.API.Features.Orders;

public record OrderCreatedEvent(
    int Id,
    string CustomerName,
    decimal Amount,
    DateTime CreatedAtUtc);
