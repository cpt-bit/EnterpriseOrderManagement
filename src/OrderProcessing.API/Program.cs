using Microsoft.EntityFrameworkCore;
using OrderProcessing.API;
using OrderProcessing.API.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. Database & Repository Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? "Data Source=orders.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));


var exCon = builder.Configuration["ExternalConfiguration"];
Console.WriteLine(exCon);

// Register EfOrderRepository to fulfill IOrderRepository
builder.Services.AddScoped<IOrderRepository, EfOrderRepository>();

// Register your OrderProcessor service (which consumes IOrderRepository via DI)
builder.Services.AddScoped<OrderProcessor>();

var app = builder.Build();

// 2. Safe Local Database Initialization
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 3. Your Existing Clean Route Grouping (Unchanged)
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

public partial class Program { }