FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
EXPOSE 80

# copy csproj and restore as distinct layers
COPY *.props .
COPY *.sln .

# Base
COPY Base.BLL/*.csproj ./Base.BLL/
COPY Base.Contracts/*.csproj ./Base.Contracts/
COPY Base.Contracts.BLL/*.csproj ./Base.Contracts.BLL/
COPY Base.Contracts.DAL/*.csproj ./Base.Contracts.DAL/
COPY Base.Contracts.Domain/*.csproj ./Base.Contracts.Domain/
COPY Base.DAL/*.csproj ./Base.DAL/
COPY Base.DAL.EF/*.csproj ./Base.DAL.EF/
COPY Base.Domain/*.csproj ./Base.Domain/
COPY Base.Helpers/*.csproj ./Base.Helpers/
COPY Base.Resources/*.csproj ./Base.Resources/

# App
COPY App.BLL/*.csproj ./App.BLL/
COPY App.Contracts.BLL/*.csproj ./App.Contracts.BLL/
COPY App.Contracts.DAL/*.csproj ./App.Contracts.DAL/
COPY App.DAL.EF/*.csproj ./App.DAL.EF/
COPY App.Domain/*.csproj ./App.Domain/
COPY App.DTO.BLL/*.csproj ./App.DTO.BLL/
COPY App.DTO.DAL/*.csproj ./App.DTO.DAL/
COPY App.DTO.Public/*.csproj ./App.DTO.Public/
COPY App.Public/*.csproj ./App.Public/
COPY App.Test/*.csproj ./App.Test/
COPY App.Constants/*.csproj ./App.Constants/
COPY WebApp/*.csproj ./WebApp/

RUN dotnet restore

# copy everything else and build app
# Base
COPY Base.BLL/. ./Base.BLL/
COPY Base.Contracts/. ./Base.Contracts/
COPY Base.Contracts.BLL/. ./Base.Contracts.BLL/
COPY Base.Contracts.DAL/. ./Base.Contracts.DAL/
COPY Base.Contracts.Domain/. ./Base.Contracts.Domain/
COPY Base.DAL/. ./Base.DAL/
COPY Base.DAL.EF/. ./Base.DAL.EF/
COPY Base.Domain/. ./Base.Domain/
COPY Base.Helpers/. ./Base.Helpers/
COPY Base.Resources/. ./Base.Resources/

# App
COPY App.BLL/. ./App.BLL/
COPY App.Contracts.BLL/. ./App.Contracts.BLL/
COPY App.Contracts.DAL/. ./App.Contracts.DAL/
COPY App.DAL.EF/. ./App.DAL.EF/
COPY App.Domain/. ./App.Domain/
COPY App.DTO.BLL/. ./App.DTO.BLL/
COPY App.DTO.DAL/. ./App.DTO.DAL/
COPY App.DTO.Public/. ./App.DTO.Public/
COPY App.Public/. ./App.Public/
COPY App.Test/. ./App.Test/
COPY App.Constants/. ./App.Constants/
COPY WebApp/. ./WebApp/

# Copy seed data
COPY App.DAL.EF/Seeding/SeedData/ /app/App.DAL.EF/Seeding/SeedData/


# Run tests
WORKDIR /src/App.Test
RUN dotnet test

# Build the application
WORKDIR /src/WebApp
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80

COPY --from=build /src/WebApp/out ./
COPY --from=build /src/App.DAL.EF/Seeding/SeedData/ ./App.DAL.EF/Seeding/SeedData/
ENV TZ=Etc/UTC

ENTRYPOINT ["dotnet", "WebApp.dll"]
