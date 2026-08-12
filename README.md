# Sucursal 360

Sucursal 360 es una aplicacion web demo para una cadena ficticia de cafeterias llamada Cafe Horizonte.
El objetivo es comparar sucursales mediante indicadores de reputacion, resenas, tendencias, categorias
manuales y metricas operativas simuladas.

El proyecto es un demo de portafolio. No es un POS, ERP, CRM, sistema de inventario, facturacion,
pagos, alertas ni clasificacion automatica con IA.

## Stack Actual

- .NET 10
- ASP.NET Core MVC con Razor Views
- ASP.NET Core Identity
- EF Core
- SQLite para desarrollo local
- MSTest

## Requisitos Locales

- .NET SDK 10
- `dotnet-ef` 10.x
- SQLite 3

Verificar herramientas:

```bash
dotnet --version
dotnet ef --version
sqlite3 --version
```

## Comandos

En este ambiente se recomienda usar `-m:1 -nr:false` para evitar problemas con workers de MSBuild.

Restaurar:

```bash
dotnet restore Sucursal360.slnx -m:1 -nr:false
```

Compilar:

```bash
dotnet build Sucursal360.slnx -m:1 -nr:false --no-restore
```

Ejecutar pruebas:

```bash
dotnet test Sucursal360.slnx -m:1 -nr:false --no-restore --no-build
```

Ejecutar la aplicacion:

```bash
dotnet run --project src/Sucursal360.Web/Sucursal360.Web.csproj
```

URLs locales:

- `http://localhost:5256`
- `https://localhost:7017`

## Base De Datos

La configuracion actual usa SQLite:

```json
"DefaultConnection": "DataSource=app.db;Cache=Shared"
```

Crear una migracion:

```bash
dotnet ef migrations add <Nombre> --project src/Sucursal360.Web/Sucursal360.Web.csproj
```

Aplicar migraciones:

```bash
dotnet ef database update --project src/Sucursal360.Web/Sucursal360.Web.csproj
```

## Documentacion Principal

- `AGENTS.md`: guia operativa para agentes de IA.
- `docs/02-alcance-objetivos.md`: alcance y objetivos.
- `docs/06-decisiones-supuestos-riesgos.md`: decisiones y riesgos.
- `docs/10-modelo-dominio-diccionario.md`: modelo de dominio.
- `docs/11-modelo-datos-dbml.md`: modelo de datos.
- `docs/15-plan-implementacion-ia.md`: plan base.
- `docs/16-plan-implementacion-ejecutable.md`: plan operativo por iteraciones.

## Siguiente Iteracion

La siguiente iteracion recomendada es crear el dominio real del demo:

- `ApplicationUser`
- entidades principales
- enums canonicos
- configuraciones EF Core
- migracion inicial de negocio
- seed de roles, categorias y cinco sucursales ficticias
