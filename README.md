# Hotel Management API

This repository contains the Hotel Management API, a robust and scalable solution for managing hotel operations. The project is structured using a layered architecture, promoting modularity, maintainability, and testability.

## Table of Contents

- [Project Structure](#project-structure)
- [Base Layer](#base-layer)
- [App Layer](#app-layer)
- [WebApp Layer](#webapp-layer)
- [Testing](#testing)
- [Getting Started](#getting-started)
- [Seeded Users](#seeded-users)
- [Contributing](#contributing)
- [License](#license)


## Important Note

While in-memory databases can be useful for certain backend operations, it is generally not recommended to use them for frontend applications. In-memory databases do not persist data across sessions, which can lead to data loss when the application is closed or refreshed. 


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
- **Middleware**: Contains custom middleware for handling X-Road specific headers and errors.

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

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request for any enhancements or bug fixes.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.   




## Useful commands in .net console CLI   

Install/update tooling

~~~bash
dotnet tool update -g dotnet-ef
~~~

~~~bash
dotnet tool update -g dotnet-aspnet-codegenerator 
~~~

## EF Core migrations

Run from solution folder

~~~bash
dotnet ef migrations --project App.DAL.EF --startup-project WebApp add First-Db
~~~
~~~bash
dotnet ef database   --project App.DAL.EF --startup-project WebApp update
~~~
~~~bash
dotnet ef database   --project App.DAL.EF --startup-project WebApp drop
~~~


## MVC controllers

These controllers are generated for quick testing of the data model.

Install from nuget:
- Microsoft.VisualStudio.Web.CodeGeneration.Design
- Microsoft.EntityFrameworkCore.SqlServer


Run from WebApp folder!

~~~bash

dotnet aspnet-codegenerator controller -name HotelsController   -actions -m  App.Domain.Hotel       -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f

dotnet aspnet-codegenerator controller -name RoomsController    -actions -m  App.Domain.Room        -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f

dotnet aspnet-codegenerator controller -name BookingsController  -actions -m  App.Domain.Booking     -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
~~~


## Api controllers
~~~bash
dotnet aspnet-codegenerator controller -name HotelsController    -m  App.Domain.Hotel       -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f

dotnet aspnet-codegenerator controller -name RoomsController     -m  App.Domain.Room        -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f

dotnet aspnet-codegenerator controller -name BookingsController  -m  App.Domain.Booking     -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
~~~



## Generate Identity UI

~~~bash
dotnet aspnet-codegenerator identity -dc AppDbContext --userClass App.Domain.Identity.AppUser -f
~~~
