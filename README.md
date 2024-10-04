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
dotnet ef migrations --project App.DAL.EF --startup-project WebApp add initial
~~~
~~~bash
dotnet ef database   --project App.DAL.EF --startup-project WebApp update
~~~
~~~bash
dotnet ef database   --project App.DAL.EF --startup-project WebApp drop
~~~
