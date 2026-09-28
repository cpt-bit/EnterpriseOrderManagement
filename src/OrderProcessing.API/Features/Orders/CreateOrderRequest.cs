using System.ComponentModel.DataAnnotations;
namespace OrderProcessing.API.Features.Orders;

public record CreateOrderRequest(
    [property: Required] string CustomerName,
    [property: Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")] decimal Amount
);
