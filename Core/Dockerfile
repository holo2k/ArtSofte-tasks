FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Core/Core.csproj Core/
RUN dotnet restore Core/Core.csproj

COPY Core/ Core/

RUN dotnet build Core/Core.csproj -c Release

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS core-base
WORKDIR /core
COPY --from=build /src/Core/bin/Release/net9.0/ .
