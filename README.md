# Hotel Management API

This repository contains the Hotel Management API, a robust and scalable solution for managing hotel operations. The project is structured using a layered architecture, promoting modularity, maintainability, and testability.

## API and Frontend Access

- **Swagger API Documentation**: Access the API documentation [here](https://hotelx-api-c9aeeebgcrbde7gt.northeurope-01.azurewebsites.net/swagger/index.html).
- **Next.js Frontend**: Access the frontend hosted in Azure [here](https://hotel-x.azurewebsites.net/).

## Table of Contents

- [Project Structure](#project-structure)
- [Base Layer](#base-layer)
- [App Layer](#app-layer)
- [WebApp Layer](#webapp-layer)
- [X-Road Protocol Implementation](#x-road-protocol-implementation)
- [Testing](#testing)
- [Getting Started](#getting-started)
- [Seeded Users](#seeded-users)
- [Contributing](#contributing)
- [License](#license)
- [Docker Guide](#docker-guide)

## X-Road Protocol Implementation

The API has been enhanced with X-Road protocol support, which includes the following features:

- **Request Header Validation**: The `X-Road-Client` header is checked for presence in incoming requests. If missing, a `BadRequest` response is returned. This is implemented in the `XRoadHeaderFilter` class.

  - Relevant Code: `XRoadHeaderFilter.cs` (startLine: 10, endLine: 18)

- **Response Headers**: The API adds several X-Road specific headers to the response:

  - `X-Road-Service`: Indicates the service being accessed, configured via the `XRoadServiceAttribute`.
  - `X-Road-Id`: A unique identifier for the request.
  - `X-Road-Request-Hash`: A hash of the request for integrity verification.
  - Relevant Code: `XRoadHeaderFilter.cs` (startLine: 20, endLine: 35)

- **Centralized Error Handling**: Custom exceptions like `BadRequestException` and `NotFoundException` are used to handle errors gracefully. The `XRoadExceptionFilter` ensures that errors are returned with a consistent structure, including an `X-Road-Error` header.

  - Relevant Code: `XRoadExceptionFilter.cs` (startLine: 10, endLine: 43), `BadRequestException.cs` (startLine: 1, endLine: 6), `NotFoundException.cs` (startLine: 1, endLine: 6)

- **Endpoint-Specific Configuration**: The `XRoadServiceAttribute` allows for endpoint-specific configuration, specifying the service details for each API endpoint.

  - Relevant Code: `XRoadServiceAttribute.cs` (startLine: 1, endLine: 12)

- **API Controllers**: The logic for handling X-Road headers and errors is integrated into the API controllers, ensuring that all endpoints comply with the X-Road protocol.
  - Relevant Code: `BookingsController.cs`, `HotelsController.cs`, `RoomsController.cs`, `AccountController.cs`, `ClientsController.cs`

## Seeded Users

The application comes with pre-configured users for testing purposes:

- **Admin User**

  - **UserName**: `admin@hotelx.com`
  - **Password**: `Foo.Bar1`

- **Guest User**
  - **UserName**: `guest@hotelx.com`
  - **Password**: `Guest.Pass1`

When registering a new user through the application, the user will be created with regular user privileges.

## Project Structure

The project is organized into several key layers:

### Base Layer

This layer contains reusable components and infrastructure shared across the application.

- **Base.Contracts**: Defines core interfaces like `IBaseEntityRepository<TEntity, TKey>` for generic repository operations and `IBaseUnitOfWork` for the Unit of Work pattern.
- **Base.DAL**: Implements base data access logic with `BaseEntityRepository<TKey, TDomainEntity, TDalEntity, TDbContext>` and `BaseUnitOfWork<TDbContext>`.
- **Base.BLL**: Implements base business logic services with `BaseEntityService<TBllEntity, TDalEntity, TRepository, TKey>`.
- **Base.Domain**: Contains base domain entity classes such as `EntityId<TKey>` and `AuditableEntity<TKey>`.
- **Base.Helpers**: Contains helper classes for common tasks, including JWT token handling.
- **Base.Resources**: Contains resource files for localization.

### App Layer

This layer contains the specific business logic and data access for the hotel management application.

- **App.Contracts.DAL**: Defines application-specific interfaces extending the base interfaces.
- **App.DAL.EF**: Implements the application's data access layer using Entity Framework Core.
- **App.BLL**: Implements the application's business logic services.
- **App.Domain**: Contains the application's domain models.
- **App.DTO**: Contains Data Transfer Objects for DAL, BLL, and Public API.

### WebApp Layer

This layer contains the ASP.NET Core Web API implementation.

- **Controllers**: Contains MVC controllers for testing purposes.
- **ApiControllers**: Contains API controllers for handling API requests.
- **Program.cs**: The application's entry point, responsible for setting up dependency injection, database configuration, and middleware.

### Testing

- **App.Test**: Contains integration tests for the API controllers.

## Getting Started

To get started with the project, follow these steps:

1. Clone the repository.
2. Set up the database and configure the connection string.
3. Run the application using your preferred IDE or command line.

### Setting Up the Database

You can use the provided `docker-compose.yaml` file to quickly set up a PostgreSQL database for the application. Ensure you have Docker and Docker Compose installed, then run the following command in the root directory of the project:

```bash
docker-compose up -d
```

This command will start a PostgreSQL container with the necessary configuration. The database will be accessible on port 5446. To stop the database, use:

```bash
docker-compose down
```

## Docker Guide

For details on building and pushing Docker images, refer to the [Dockerfile](Dockerfile) and [DOCKER_GUIDE.md](DOCKER_GUIDE.md).

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request for any enhancements or bug fixes.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Useful commands in .net console CLI

Install/update tooling

```bash
dotnet tool update -g dotnet-ef
```

```bash
dotnet tool update -g dotnet-aspnet-codegenerator
```

## EF Core migrations

Run from solution folder

```bash
dotnet ef migrations --project App.DAL.EF --startup-project WebApp add First-Db
```

```bash
dotnet ef database   --project App.DAL.EF --startup-project WebApp update
```

```bash
dotnet ef database   --project App.DAL.EF --startup-project WebApp drop
```

## MVC controllers

These controllers are generated for quick testing of the data model.

Install from nuget:

- Microsoft.VisualStudio.Web.CodeGeneration.Design
- Microsoft.EntityFrameworkCore.SqlServer

Run from WebApp folder!

```bash

dotnet aspnet-codegenerator controller -name HotelsController   -actions -m  App.Domain.Hotel       -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f

dotnet aspnet-codegenerator controller -name RoomsController    -actions -m  App.Domain.Room        -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f

dotnet aspnet-codegenerator controller -name BookingsController  -actions -m  App.Domain.Booking     -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
```

## Api controllers

```bash
dotnet aspnet-codegenerator controller -name HotelsController    -m  App.Domain.Hotel       -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f

dotnet aspnet-codegenerator controller -name RoomsController     -m  App.Domain.Room        -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f

dotnet aspnet-codegenerator controller -name BookingsController  -m  App.Domain.Booking     -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
```

## Generate Identity UI

```bash
dotnet aspnet-codegenerator identity -dc AppDbContext --userClass App.Domain.Identity.AppUser -f
```
