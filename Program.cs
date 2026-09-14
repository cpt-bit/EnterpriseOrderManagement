using OrderProcessing.API;

var builder = WebApplication.CreateBuilder(args);

// API Documentation Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dependency Injection Registrations
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddScoped<OrderProcessor>();

var app = builder.Build();

// HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// API Routing
app.MapGet("/orders/{id:int}", async (int id, OrderProcessor processor) =>
{
    var order = await processor.ProcessOrderAsync(id);
    return order is not null ? Results.Ok(order) : Results.NotFound();
});

app.Run();