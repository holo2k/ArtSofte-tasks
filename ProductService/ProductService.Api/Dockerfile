# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем все проекты сразу, сохранив структуру
COPY Core/ Core/
COPY ProductService/ProductService.DAL/ ProductService/ProductService.DAL/
COPY ProductService/ProductService.Logic/ ProductService/ProductService.Logic/
COPY ProductService/ProductService.Api/ ProductService/ProductService.Api/

# Восстанавливаем зависимости Api проекта (включает ссылки на Core, Logic, DAL)
RUN dotnet restore ProductService/ProductService.Api/ProductService.Api.csproj

# Публикуем Api проект
WORKDIR /src/ProductService/ProductService.Api
RUN dotnet publish ProductService.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish/ .

ENTRYPOINT ["dotnet", "ProductService.Api.dll"]
