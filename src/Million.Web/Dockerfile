FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./Million.sln ./
COPY ./src/Million.Domain/Million.Domain.csproj ./src/Million.Domain/
COPY ./src/Million.Application/Million.Application.csproj ./src/Million.Application/
COPY ./src/Million.Infrastructure/Million.Infrastructure.csproj ./src/Million.Infrastructure/
COPY ./src/Million.Web/Million.Web.csproj ./src/Million.Web/
RUN dotnet restore Million.sln
COPY . .
RUN dotnet publish src/Million.Web/Million.Web.csproj -c Release -o /out

FROM base AS final
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "Million.Web.dll"]

