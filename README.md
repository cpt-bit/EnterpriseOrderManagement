# Enterprise Order Management System

A production-grade, cloud-native .NET 8 Minimal API demonstrating senior-level architecture, event-driven messaging, robust data validation, and distributed orchestration.

## 🚀 Tech Stack & Architecture

* **Framework:** .NET 8 (Minimal APIs)
* **Cloud Orchestration:** .NET Aspire (`EnterpriseOrderManagement.AppHost`)
* **Messaging & Events:** MassTransit, RabbitMQ (with EF Core Transactional Outbox Pattern)
* **Persistence:** SQLite, Entity Framework Core 8
* **Validation & Error Handling:** Custom Endpoint Filters, Data Annotations, RFC 9110 Problem Details (`ValidationProblem`)
* **Testing:** xUnit, FluentAssertions, NSubstitute
* **Observability:** OpenTelemetry (Metrics, Tracing, Logging) via Aspire Dashboard
* **Containerization:** Docker support

---

## 🏛️ Key Architectural Features

* **Clean Architecture & Feature Slicing:** Organized by business features (`Features/Orders/`) keeping endpoints, processors, and models cohesively grouped.
* **Resilient Event-Driven Messaging:** Integrates MassTransit with RabbitMQ backed by the EF Core Transactional Outbox pattern to guarantee reliable event publishing without dual-write hazards.
* **Advanced Validation Pipeline:** Overcomes out-of-the-box Minimal API limitations by using a reusable `ValidationFilter<T>` that automatically enforces record-level `[Required]` and `[Range]` Data Annotations, returning standardized `400 ValidationProblem` responses.
* **Centralized Global Exception Handling:** Translates unhandled exceptions into RFC-compliant problem details with correlation IDs and trace tracking.
* **Comprehensive Test Suite:** Includes isolated unit tests for the domain processor using NSubstitute mocks and repository tests leveraging in-memory SQLite instances.

---

## 📦 Project Structure

* `EnterpriseOrderManagement.AppHost`: .NET Aspire orchestration project managing containers (RabbitMQ) and service references.
* `EnterpriseOrderManagement.ServiceDefaults`: Shared cross-cutting configurations for OpenTelemetry, health checks, and service discovery.
* `OrderProcessing.API`: Core Minimal API web project containing endpoints, features, middleware, and database migrations.
* `OrderProcessing.Tests`: Unit and repository test suites using xUnit and FluentAssertions.

---

## 🛠️ Getting Started

### Prerequisites
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) (required for .NET Aspire container orchestration)

### Running the Application
You can run the entire solution via the Aspire AppHost or run the API directly:

1. **Using .NET Aspire (Recommended):**
   ```powershell
   dotnet run --project src/EnterpriseOrderManagement.AppHost/EnterpriseOrderManagement.AppHost.csproj