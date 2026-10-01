FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/PartnerSystem.Shared/PartnerSystem.Shared.csproj PartnerSystem.Shared/
COPY src/PartnerSystem.Contracts/PartnerSystem.Contracts.csproj PartnerSystem.Contracts/
COPY src/PartnerSystem.WalletService/PartnerSystem.WalletService.csproj PartnerSystem.WalletService/

RUN dotnet restore PartnerSystem.WalletService/PartnerSystem.WalletService.csproj

COPY src/ .

RUN dotnet publish PartnerSystem.WalletService/PartnerSystem.WalletService.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PartnerSystem.WalletService.dll"]