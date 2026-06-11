# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/src.sln ./
COPY src/Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj Fundo.Applications.WebApi/
COPY src/Fundo.Services.Tests/Fundo.Services.Tests.csproj Fundo.Services.Tests/

RUN dotnet restore Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj

COPY src/ ./
RUN dotnet publish Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "Fundo.Applications.WebApi.dll"]
