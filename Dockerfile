# Multi-stage build para optimizar el tamaño final
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["src/Million.Web/Million.Web.csproj", "src/Million.Web/"]
COPY ["src/Million.Application/Million.Application.csproj", "src/Million.Application/"]
COPY ["src/Million.Infrastructure/Million.Infrastructure.csproj", "src/Million.Infrastructure/"]
COPY ["src/Million.Domain/Million.Domain.csproj", "src/Million.Domain/"]

# Restaurar dependencias
RUN dotnet restore "src/Million.Web/Million.Web.csproj"

# Copiar todo el código fuente
COPY . .

# Build de la aplicación
WORKDIR "/src/src/Million.Web"
RUN dotnet build "Million.Web.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Million.Web.csproj" -c Release -o /app/publish

# Runtime stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Variables de entorno por defecto
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80;https://+:443

ENTRYPOINT ["dotnet", "Million.Web.dll"]

