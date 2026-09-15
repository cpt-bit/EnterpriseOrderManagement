using OrderProcessing.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddScoped<OrderProcessor>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/orders/{id:int}", async (int id, OrderProcessor processor) =>
{
    var order = await processor.ProcessOrderAsync(id);
    return order is not null ? Results.Ok(order) : Results.NotFound();
});

app.Run();