# Build multi-stage de ApiMotos (.NET 8).
# Contexto de build: la raíz de motos/ (ver docker-compose.yml -> api.build.context).

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore primero (capa cacheable): solo el csproj.
COPY src/ApiMotos/ApiMotos.csproj src/ApiMotos/
RUN dotnet restore src/ApiMotos/ApiMotos.csproj

# Resto del código y publish.
COPY src/ src/
RUN dotnet publish src/ApiMotos/ApiMotos.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# Scripts/*.sql (FK_* y seeds Cfg_*) los aplica DbBootstrap al arrancar desde
# ContentRootPath/Scripts. dotnet publish no los incluye, así que se copian aparte
# (sin tocar el .csproj, para no alterar el build local).
COPY src/ApiMotos/Scripts ./Scripts
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ApiMotos.dll"]
