# Sucursal 360 Agent Guide

## Project Identity

Sucursal 360 is a portfolio/demo web application for the fictitious coffee chain Cafe Horizonte.
It helps management compare branch reputation, review themes, trends, and simulated operational
metrics. It is not connected to any real company data.

## Source Of Truth

When product or technical decisions conflict, use this order:

1. `docs/06-decisiones-supuestos-riesgos.md`
2. `docs/02-alcance-objetivos.md`
3. `docs/04-requisitos-negocio.md`
4. `docs/05-integraciones-kpi-datos.md`
5. `docs/07-investigacion-tecnica-integracion.md`
6. `docs/10-modelo-dominio-diccionario.md`
7. `docs/11-modelo-datos-dbml.md`
8. `docs/12-arquitectura-solucion.md`
9. `docs/13-diseno-integraciones-contratos.md`
10. `docs/14-diseno-seguridad-acceso.md`
11. `docs/15-plan-implementacion-ia.md`
12. `docs/16-plan-implementacion-ejecutable.md`

If a change would alter scope, data policy, provider behavior, packages, security rules, or cloud
strategy, stop and ask before implementing.

## Current Local Decisions

- Use .NET 10.
- Use ASP.NET Core MVC with Razor Views.
- Use EF Core.
- Use SQLite for local development and early demo work.
- Keep the EF model portable enough to move to SQL Server/Azure SQL later.
- Use MSTest for tests.
- Build the required provider with controlled demo data first.
- Treat live providers such as Geoapify or Google Places as optional later integrations.

## Language And Naming

- Documentation and UI text: Spanish.
- Code identifiers, classes, interfaces, methods, properties, and folders: English.
- Role names are exact application constants:
  - `Administrador`
  - `GerenteCorporativo`
  - `GerenteSucursal`
- Important UI labels:
  - `Datos simulados`
  - `No disponible`
  - `Clasificacion manual`
  - `Datos en vivo`

## Architecture Rules

- Keep one production project: `src/Sucursal360.Web`.
- Keep one test project: `tests/Sucursal360.Tests`.
- Do not introduce Clean Architecture, CQRS, MediatR, AutoMapper, generic repositories,
  microservices, queues, distributed cache, or a separate SPA in V1.
- Add abstractions only at real boundaries:
  - `IPublicBranchDataProvider`
  - `IBranchSynchronizationService`
  - `IBranchAccessService`
  - `IReviewCategorizationService`
  - `ISimulatedDataImportService`
  - `IManagementReportExporter`
- Controllers should coordinate HTTP and call services. They should not contain large EF queries
  or business rules.
- Services may use `ApplicationDbContext` directly with explicit queries.
- Do not expose EF entities directly as form models. Use ViewModels for user input.

## Scope Boundaries

Required V1 capabilities:

- Local login with roles.
- Branch catalog and active/inactive state.
- Server-side branch scope enforcement.
- Demo public-data synchronization using local fixtures.
- Integration run log with safe messages and correlation IDs.
- Corporate dashboard.
- Branch detail and history.
- Review filtering and manual category assignment.
- Simulated POS/ERP CSV import.
- Management Excel export.

Out of scope for V1:

- Real POS or ERP integration.
- POS screens, orders, payments, invoicing, inventory, products, customers.
- Alerts, tasks, workflow management, email notifications.
- AI sentiment or automatic review classification.
- Automatic scheduled sync.
- Mobile native app.
- PDF export.

## Data And Provider Rules

- Demo provider data may be persisted as snapshots and reviews.
- Google Places content must not be persisted by default. Store only place IDs and integration
  diagnostics unless the docs are updated after a policy review.
- Missing external values render as `No disponible`, never as zero.
- Simulated operational metrics must always be labeled `Datos simulados`.
- Do not use real company names, logos, internal data, or secrets.
- Do not use scraping.

## Security Rules

- Use ASP.NET Core Identity with roles.
- No self-registration in V1.
- Enforce authorization on the server for every branch resource.
- A `GerenteSucursal` may access only the assigned branch.
- Admin-only POST endpoints must require authorization and antiforgery.
- Never log passwords, cookies, API keys, connection strings, raw external response bodies,
  or full review text.
- Neutralize Excel text values that start with `=`, `+`, `-`, `@`, tab, or carriage return.

## Local Commands

Use these commands in this environment because the default MSBuild worker behavior can hang or
hit local socket restrictions:

```bash
dotnet restore Sucursal360.slnx -m:1 -nr:false
dotnet build Sucursal360.slnx -m:1 -nr:false --no-restore
dotnet test Sucursal360.slnx -m:1 -nr:false --no-restore --no-build
```

For EF migrations:

```bash
dotnet ef migrations add <Name> --project src/Sucursal360.Web/Sucursal360.Web.csproj
dotnet ef database update --project src/Sucursal360.Web/Sucursal360.Web.csproj
```

Run locally:

```bash
dotnet run --project src/Sucursal360.Web/Sucursal360.Web.csproj
```

Default local URLs from launch settings:

- `http://localhost:5256`
- `https://localhost:7017`

## Implementation Style

- Work in small vertical slices.
- Prefer shipping a visible, testable path over building broad unused infrastructure.
- Update documentation when a decision changes.
- Add tests around invariants, authorization, provider mapping, import validation, and report safety.
- Do not commit unless the user explicitly asks.
