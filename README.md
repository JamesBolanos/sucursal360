# Sucursal 360

Sucursal 360 es una aplicacion web local-first en ASP.NET Core MVC para una cadena ficticia de cafeterias llamada Cafe Horizonte. El demo consolida reputacion publica, categorias manuales de resenas, metricas POS/ERP simuladas y reportes Excel.

Este es un proyecto de portafolio/demo. No es un POS, ERP, CRM, sistema de inventario, facturacion, pagos, alertas, gestion de tareas ni analisis automatico de sentimiento con IA.

## Stack

- .NET 10
- ASP.NET Core MVC con Razor Views
- ASP.NET Core Identity
- EF Core con SQLite para desarrollo local
- ClosedXML para exportacion Excel
- MSTest

## Herramientas Locales

```bash
dotnet --version
dotnet ef --version
sqlite3 --version
```

Esperado: .NET SDK 10.x, `dotnet-ef` 10.x y SQLite 3.

## Configuracion

Restaurar, compilar y aplicar migraciones:

```bash
dotnet restore Sucursal360.slnx -m:1 -nr:false
dotnet build Sucursal360.slnx -m:1 -nr:false --no-restore
dotnet ef database update --project src/Sucursal360.Web/Sucursal360.Web.csproj
```

Configurar la contrasena local de usuarios demo con User Secrets:

```bash
dotnet user-secrets set "SeedUsers:DefaultPassword" "<tu-password-local>" --project src/Sucursal360.Web/Sucursal360.Web.csproj
```

Politica de contrasena: al menos 10 caracteres, mayuscula, minuscula y digito. Caracter no alfanumerico es opcional.

Ejecutar la aplicacion:

```bash
dotnet run --project src/Sucursal360.Web/Sucursal360.Web.csproj
```

URLs locales comunes:

- `http://localhost:5256`
- `https://localhost:7017`

## Usuarios Demo

La aplicacion crea o actualiza estos usuarios solo en ambiente `Development` y solo cuando existe `SeedUsers:DefaultPassword`:

- `admin@sucursal360.local` -> `Administrador`
- `corporativo@sucursal360.local` -> `GerenteCorporativo`
- `sucursal@sucursal360.local` -> `GerenteSucursal`, asignado a `SUC-001`

## Flujo Demo

1. Iniciar sesion como `admin@sucursal360.local`.
2. Abrir `Integraciones`.
3. Click en `Sincronizar todas` para cargar datos publicos demo.
4. Abrir `Panel` para comparar sucursales, ranking e insights.
5. Click en `Ver` sobre una sucursal para revisar historial y ultima sincronizacion.
6. Abrir `Resenas` para filtrar comentarios y asignar categorias manuales.
7. Abrir `Datos simulados`.
8. Subir [samples/simulated-operational-metrics.csv](samples/simulated-operational-metrics.csv), validar y confirmar.
9. Volver a `Panel` y al detalle de sucursal para ver metricas operativas.
10. Abrir `Reportes` y exportar el libro Excel gerencial.

## Notas De Datos

- Cafe Horizonte, sucursales, snapshots publicos, resenas y valores operativos son ficticios.
- Las categorias de resenas son temas, no sentimiento. Las estrellas indican si la resena es positiva o negativa.
- Las metricas operativas siempre se muestran como `Datos simulados`.
- Google Places no se usa en el recorrido local por defecto.
- Los archivos SQLite locales se ignoran por Git mediante `src/**/app.db*`.

## Verificacion

Compilar:

```bash
dotnet build Sucursal360.slnx -m:1 -nr:false --no-restore
```

Ejecutar pruebas:

```bash
dotnet test Sucursal360.slnx -m:1 -nr:false --no-restore --no-build
```

## Rutas Importantes

- `src/Sucursal360.Web`: aplicacion MVC
- `tests/Sucursal360.Tests`: suite MSTest
- `samples/simulated-operational-metrics.csv`: CSV demo para importacion
- `docs/16-plan-implementacion-ejecutable.md`: plan por iteraciones
- `docs/17-guion-demo.md`: guion demo de 5-7 minutos
- `AGENTS.md`: guia operativa para agentes de IA
