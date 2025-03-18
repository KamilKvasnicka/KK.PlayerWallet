FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

RUN apt-get update && apt-get install -y curl

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY KK.WalletService/KK.WalletService.csproj KK.WalletService/
RUN dotnet restore KK.WalletService/KK.WalletService.csproj

COPY . . 
WORKDIR /src/KK.WalletService
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "KK.WalletService.dll"]
