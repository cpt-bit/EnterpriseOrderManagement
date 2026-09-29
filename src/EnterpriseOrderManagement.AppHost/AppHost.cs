var builder = DistributedApplication.CreateBuilder(args);

// Add the RabbitMQ container resource
var rabbitmq = builder.AddRabbitMQ("messaging")
    .WithManagementPlugin();// Enables the web UI container endpoint

// Add your API and automatically inject the RabbitMQ connection string
builder.AddProject<Projects.OrderProcessing_API>("orderprocessing-api")
       .WithReference(rabbitmq)
       .WithHttpsEndpoint(port: 7258, name: "https");// Keeps port static

builder.Build().Run();