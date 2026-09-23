using MassTransit;
using Microsoft.Extensions.Logging;

namespace OrderProcessing.API.Features.Orders;

public class OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger) : IConsumer<OrderCreatedEvent>
{
    public Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("RECEIVED EVENT: Order {Id} created for {CustomerName} at {CreatedAtUtc} with amount ${Amount}",
            message.Id, message.CustomerName, message.CreatedAtUtc, message.Amount);

        return Task.CompletedTask;
    }
}