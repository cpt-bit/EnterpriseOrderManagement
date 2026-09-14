# Enterprise Order Management System

A production-grade .NET 8 Minimal API demonstrating clean architecture, dependency injection, and automated unit testing practices.

## Tech Stack
* **Framework:** .NET 8 (Minimal APIs)
* **API Documentation:** OpenAPI / Swashbuckle
* **Testing:** xUnit, NSubstitute
* **Development Environment:** JetBrains Rider

## Architecture & Design Patterns
* **Dependency Injection:** Loose coupling via interface abstraction (`IOrderRepository`).
* **Unit Testing:** AAA (Arrange-Act-Assert) pattern using NSubstitute for mock isolation.
* **OpenAPI/Swagger:** Standardized schema generation pinned to stable framework dependencies.

## Getting Started

### Prerequisites
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Run the API
```powershell
dotnet restore
dotnet run --project OrderProcessing.API.csproj
