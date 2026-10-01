FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/PartnerSystem.Shared/PartnerSystem.Shared.csproj PartnerSystem.Shared/
COPY src/PartnerSystem.Contracts/PartnerSystem.Contracts.csproj PartnerSystem.Contracts/
COPY src/PartnerSystem.AdminService/PartnerSystem.AdminService.csproj PartnerSystem.AdminService/

RUN dotnet restore PartnerSystem.AdminService/PartnerSystem.AdminService.csproj

COPY src/ .

RUN dotnet publish PartnerSystem.AdminService/PartnerSystem.AdminService.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PartnerSystem.AdminService.dll"]