FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["tests/PartnerSystem.Tests/PartnerSystem.Tests.csproj", "tests/PartnerSystem.Tests/"]
COPY ["src/PartnerSystem.Shared/PartnerSystem.Shared.csproj", "src/PartnerSystem.Shared/"]
COPY ["src/CommissionService/CommissionService.csproj", "src/CommissionService/"]
RUN dotnet restore "tests/PartnerSystem.Tests/PartnerSystem.Tests.csproj"
COPY . .
WORKDIR "/src/tests/PartnerSystem.Tests"
RUN dotnet build "./PartnerSystem.Tests.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PartnerSystem.Tests.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PartnerSystem.Tests.dll"]
