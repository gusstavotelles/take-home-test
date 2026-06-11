# Backend — Loan Management API

.NET 10 Web API for managing loans. Uses Entity Framework Core with SQL Server (or in-memory for development) and exposes Swagger documentation.

## Stack

- ASP.NET Core 10 (controllers)
- Entity Framework Core 10 (SQL Server / InMemory)
- FluentValidation
- Serilog (structured console logging)
- Swashbuckle / Swagger UI
- xUnit + Microsoft.AspNetCore.Mvc.Testing for unit and integration tests

## Project structure

```
backend/
  Dockerfile
  src/
    Fundo.Applications.WebApi/
      Controllers/        # HTTP layer
      Services/           # Business logic
      Data/               # EF Core DbContext + seeder
      Domain/             # Entities and value rules
      DTOs/               # Request/response contracts
      Validators/         # FluentValidation rules
      Middleware/         # Global exception handler
      Exceptions/         # Domain exceptions
    Fundo.Services.Tests/
      Unit/               # Service-level unit tests
      Integration/        # API tests via WebApplicationFactory
```

## Endpoints

| Method | Path                       | Description                         |
| ------ | -------------------------- | ----------------------------------- |
| GET    | `/loans`                   | List all loans                      |
| GET    | `/loans/{id}`              | Get loan by id                      |
| POST   | `/loans`                   | Create a new loan                   |
| POST   | `/loans/{id}/payment`      | Apply a payment to a loan           |
| GET    | `/health`                  | Health probe                        |
| GET    | `/swagger`                 | OpenAPI documentation               |

## Running locally (in-memory)

Default `appsettings.Development.json` uses an in-memory database, so no SQL Server is required to develop or run the test suite.

```bash
cd backend/src
dotnet restore
dotnet run --project Fundo.Applications.WebApi
```

The API will be available at `http://localhost:5000` and Swagger at `http://localhost:5000/swagger`.

## Running with Docker Compose (SQL Server)

From the repository root:

```bash
docker compose up --build
```

This starts SQL Server 2022 and the API. The API will create the schema on startup and seed sample loans.

- API: `http://localhost:5000`
- SQL Server: `localhost,1433` — user `sa` / password `Your_password123!`

## Running tests

```bash
cd backend/src
dotnet test
```

## Configuration

| Key                               | Description                                     |
| --------------------------------- | ----------------------------------------------- |
| `UseInMemoryDatabase`             | When `true`, uses EF Core InMemory provider     |
| `ConnectionStrings:LoanDb`        | SQL Server connection string                    |
| `Cors:AllowedOrigins`             | Allowed origins for the Angular frontend        |
