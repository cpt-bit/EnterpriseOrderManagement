using OrderProcessing.API.Data;
using OrderProcessing.API.Extensions;
using OrderProcessing.API.Features.Orders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);

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

app.MapOrderEndpoints();

app.Run();

public partial class Program { }