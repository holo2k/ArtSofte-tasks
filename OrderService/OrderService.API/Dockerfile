# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем все проекты сразу, сохранив структуру
COPY Core/ Core/
COPY OrderService/OrderService.Domain/ OrderService/OrderService.Domain/
COPY OrderService/OrderService.Infrastructure/ OrderService/OrderService.Infrastructure/
COPY OrderService/OrderService.Application/ OrderService/OrderService.Application/
COPY OrderService/OrderService.API/ OrderService/OrderService.API/

# Восстанавливаем зависимости Api проекта (включает ссылки на Core, Logic, DAL)
RUN dotnet restore OrderService/OrderService.API/OrderService.API.csproj

# Публикуем Api проект
WORKDIR /src/OrderService/OrderService.API
RUN dotnet publish OrderService.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish/ .

ENTRYPOINT ["dotnet", "OrderService.API.dll"]