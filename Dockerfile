FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore GhostNetwork.Account.Web/GhostNetwork.Account.Web.csproj
WORKDIR /src/GhostNetwork.Account.Web
RUN dotnet build GhostNetwork.Account.Web.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish GhostNetwork.Account.Web.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "GhostNetwork.Account.Web.dll"]
