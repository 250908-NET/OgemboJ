# MinimalApiDemo

An ASP.NET Core **Minimal API** that communicates over HTTP(S).  
This repo will evolve step by step to include:
- DTOs
- EF Core with SQL Server running in Docker
- Many-to-many models + ERD
- Unit tests with 20%+ coverage
- Required endpoints (GET, POST, DELETE)

## Tech
- .NET 8+ (SDK)
- ASP.NET Core Minimal API

## Getting Started

### Prerequisites
- [.NET SDK 8 or newer](https://dotnet.microsoft.com/download)

### Run
```bash
dotnet build
dotnet run --project MinimalApi/MinimalApi.csproj
