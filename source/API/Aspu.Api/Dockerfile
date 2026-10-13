# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["generators/SourceGenerators.Domain/SourceGenerators.Domain.csproj", "generators/SourceGenerators.Domain/"]
COPY ["generators/SourceGenerators.Application/SourceGenerators.Application.csproj", "generators/SourceGenerators.Application/"]
COPY ["generators/SourceGenerators.Presentation/SourceGenerators.Presentation.csproj", "generators/SourceGenerators.Presentation/"]
COPY ["source/Common/Aspu.Common.Domain/Aspu.Common.Domain.csproj", "source/Common/Aspu.Common.Domain/"]
COPY ["source/Common/Aspu.Common.Application/Aspu.Common.Application.csproj", "source/Common/Aspu.Common.Application/"]
COPY ["source/Common/Aspu.Common.Infrastructure/Aspu.Common.Infrastructure.csproj", "source/Common/Aspu.Common.Infrastructure/"]
COPY ["source/Common/Aspu.Common.Presentation/Aspu.Common.Presentation.csproj", "source/Common/Aspu.Common.Presentation/"]
COPY ["source/Modules/Orders/Aspu.Modules.Orders.Domain/Aspu.Modules.Orders.Domain.csproj", "source/Modules/Orders/Aspu.Modules.Orders.Domain/"]
COPY ["source/Modules/Orders/Aspu.Modules.Orders.Application/Aspu.Modules.Orders.Application.csproj", "source/Modules/Orders/Aspu.Modules.Orders.Application/"]
COPY ["source/Modules/Orders/Aspu.Modules.Orders.IntegrationEvents/Aspu.Modules.Orders.IntegrationEvents.csproj", "source/Modules/Orders/Aspu.Modules.Orders.IntegrationEvents/"]
COPY ["source/Modules/Orders/Aspu.Modules.Orders.Infrastructure/Aspu.Modules.Orders.Infrastructure.csproj", "source/Modules/Orders/Aspu.Modules.Orders.Infrastructure/"]
COPY ["source/Modules/Orders/Aspu.Modules.Orders.PublicApi/Aspu.Modules.Orders.PublicApi.csproj", "source/Modules/Orders/Aspu.Modules.Orders.PublicApi/"]
COPY ["source/Modules/Orders/Aspu.Modules.Orders.Presentation/Aspu.Modules.Orders.Presentation.csproj", "source/Modules/Orders/Aspu.Modules.Orders.Presentation/"]
COPY ["source/API/Aspu.Api/Aspu.Api.csproj", "source/API/Aspu.Api/"]
RUN dotnet restore "./source/API/Aspu.Api/Aspu.Api.csproj"
COPY . .
WORKDIR "/src/source/API/Aspu.Api"
RUN dotnet build "./Aspu.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Aspu.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Aspu.Api.dll"]