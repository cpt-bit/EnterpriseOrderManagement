namespace OrderProcessing.API.Features.Orders;

public record CreateOrderRequest(string CustomerName, decimal Amount);
