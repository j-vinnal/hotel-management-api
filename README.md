# hotel-management-api


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
dotnet ef migrations --project App.DAL.EF --startup-project WebApp add initial-db
~~~
~~~bash
dotnet ef database   --project App.DAL.EF --startup-project WebApp update
~~~
~~~bash
dotnet ef database   --project App.DAL.EF --startup-project WebApp drop
~~~


# MVC controllers

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


#Generate Identity UI

~~~bash
dotnet aspnet-codegenerator identity -dc AppDbContext --userClass App.Domain.Identity.AppUser -f
~~~

