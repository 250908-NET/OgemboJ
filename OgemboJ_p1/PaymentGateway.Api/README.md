# PaymentGateway Minimal API

A minimal ASP.NET Core API that simulates a simple payment gateway.

## Features
- ASP.NET Core **Minimal API** (.NET 9)
- **EF Core** with SQL Server (Docker)
- **DTOs** for request/response
- **GET, POST, DELETE** endpoints
- **Many-to-Many**: Merchant ↔ PaymentMethod via MerchantPaymentMethod
- **2+ models**: Merchant, Customer, Payment, PaymentMethod
- **Unit tests** (xUnit, InMemory EF) targeting ≥20% coverage
- **Swagger** for interactive docs
- ERD included (Mermaid)

## Quick Start
1. `docker compose up -d` (SQL Server)
2. `dotnet ef database update --project PaymentGateway.Api`
3. `dotnet run --project PaymentGateway.Api`
4. Open Swagger: `https://localhost:5001/swagger`
5. Seed data (dev only): `POST /dev/seed`
6. Create a payment: `POST /payments`
7. Get a payment: `GET /payments/{id}`
8. Delete (refund+remove): `DELETE /payments/{id}`

## Connection String
See `appsettings.json` → `ConnectionStrings:Default`. The SA password must meet SQL Server complexity rules.

## Testing
`dotnet test` (uses EFCore.InMemory).

## ERD


erDiagram
    Merchant ||--o{ Payment : has
    Customer ||--o{ Payment : makes
    PaymentMethod ||--o{ MerchantPaymentMethod : "supported by"
    Merchant ||--o{ MerchantPaymentMethod : "supports"

    Merchant {
      GUID Id PK
      string Name
      string ApiKey
    }

    Customer {
      GUID Id PK
      string Email
      string FullName
    }

    PaymentMethod {
      int Id PK
      string Code
      string DisplayName
    }

    MerchantPaymentMethod {
      GUID MerchantId FK
      int PaymentMethodId FK
    }

    Payment {
      GUID Id PK
      GUID MerchantId FK
      GUID CustomerId FK
      int PaymentMethodId FK
      decimal Amount
      string Currency
      string Status
      datetime CreatedAt
    }
