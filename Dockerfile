FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Sucursal360.slnx ./
COPY src/Sucursal360.Web/Sucursal360.Web.csproj src/Sucursal360.Web/
COPY tests/Sucursal360.Tests/Sucursal360.Tests.csproj tests/Sucursal360.Tests/
RUN dotnet restore Sucursal360.slnx -m:1 -nr:false

COPY . .
RUN dotnet publish src/Sucursal360.Web/Sucursal360.Web.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
COPY samples ./samples

RUN mkdir -p /tmp/sucursal360

ENV ASPNETCORE_ENVIRONMENT=Production

CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet Sucursal360.Web.dll"]
