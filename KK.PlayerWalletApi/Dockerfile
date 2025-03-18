FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5003

RUN apt-get update && apt-get install -y curl wget

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY KK.PlayerWalletApi/KK.PlayerWalletApi.csproj KK.PlayerWalletApi/
RUN dotnet restore KK.PlayerWalletApi/KK.PlayerWalletApi.csproj

COPY . .
WORKDIR /src/KK.PlayerWalletApi
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app

RUN apt-get update && apt-get install -y curl wget

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KK.PlayerWalletApi.dll"]
