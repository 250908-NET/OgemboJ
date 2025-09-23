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


## Entity Relationship Diagram
erDiagram
    STUDENTS ||--o{ ENROLLMENTS : "has many"
    COURSES  ||--o{ ENROLLMENTS : "has many"

    STUDENTS {
        int Id PK
        string FullName
        string Email
        datetime CreatedAt
    }

    COURSES {
        int Id PK
        string Title
        string Code
        int Credits
    }

    ENROLLMENTS {
        int StudentId FK
        int CourseId  FK
        datetime EnrolledOn
        string Grade
    }
