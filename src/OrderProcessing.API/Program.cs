using OrderProcessing.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// InMemory repository stays Singleton so state persists between API calls
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddScoped<OrderProcessor>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Route Grouping (.NET 8 Pattern)
var ordersGroup = app.MapGroup("/api/orders");

// GET /api/orders/{id}
ordersGroup.MapGet("/{id:int}", async (int id, OrderProcessor processor) =>
{
    var order = await processor.ProcessOrderAsync(id);
    return order is not null 
        ? Results.Ok(order) 
        : Results.NotFound($"Order {id} not found.");
});

// POST /api/orders/{id}/discount
ordersGroup.MapPost("/{id:int}/discount", async (int id, decimal percent, OrderProcessor processor) =>
{
    try
    {
        var success = await processor.ApplyDiscountAsync(id, percent);
        return success 
            ? Results.NoContent() 
            : Results.NotFound($"Order {id} not found.");
    }
    catch (ArgumentOutOfRangeException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.Run();