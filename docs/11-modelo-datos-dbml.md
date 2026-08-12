# Sucursal 360

## Modelo de datos y esquema DBML

| Campo | Valor |
|---|---|
| Versión | 1.0 |
| Fecha | 12 de agosto de 2026 |
| Estado | Línea base lógica; motor propuesto SQL Server 2022 |

## 1. Propósito

Especificar las tablas de negocio, claves, restricciones e índices suficientes para generar el modelo EF Core. EF Core Migrations es la fuente de verdad física durante la implementación; este DBML es la fuente de verdad lógica y debe actualizarse cuando cambie el modelo.

## 2. Convenciones

- Tablas y columnas físicas usan `PascalCase` para alinearse con EF Core/Identity.
- PK de negocio: `uniqueidentifier`/`Guid` generado por aplicación.
- PK de Identity: `nvarchar(450)` gestionado por Identity.
- Fechas técnicas: `datetimeoffset` en UTC.
- Fecha de negocio: `date`.
- Valores monetarios: `decimal(18,2)`.
- Borrado físico: `Restrict`; no usar cascade sobre históricos.
- Enums: `int` con validación de aplicación y `CHECK` creado por migración donde sea simple.

## 3. DBML canónico

```dbml
Project Sucursal360 {
  database_type: 'SQL Server'
  Note: 'Modelo demo. Identity crea tablas adicionales de claims, logins y tokens.'
}

Table Branches {
  Id uniqueidentifier [pk]
  Code nvarchar(20) [not null, unique]
  Name nvarchar(120) [not null]
  IsActive bit [not null, default: 1]
  Provider int [not null, note: '1 Demo; 2 GooglePlaces']
  ExternalPlaceId nvarchar(200)
  CreatedAtUtc datetimeoffset [not null]
  UpdatedAtUtc datetimeoffset [not null]

  indexes {
    (Provider, ExternalPlaceId) [unique, name: 'UX_Branches_Provider_ExternalPlaceId']
    IsActive [name: 'IX_Branches_IsActive']
  }
}

Table BranchSnapshots {
  Id uniqueidentifier [pk]
  BranchId uniqueidentifier [not null]
  Provider int [not null, note: 'V1 persistida: 1 Demo']
  DisplayName nvarchar(160)
  Address nvarchar(300)
  Latitude decimal(9,6)
  Longitude decimal(9,6)
  BusinessStatus int
  OpeningHoursJson nvarchar(max)
  Rating decimal(2,1)
  ReviewCount int
  RetrievedAtUtc datetimeoffset [not null]
  IntegrationRunId uniqueidentifier [not null]

  indexes {
    (BranchId, Provider, RetrievedAtUtc) [unique, name: 'UX_BranchSnapshots_Branch_Provider_Date']
    (BranchId, RetrievedAtUtc) [name: 'IX_BranchSnapshots_Branch_Date']
  }
}

Table Reviews {
  Id uniqueidentifier [pk]
  BranchId uniqueidentifier [not null]
  Provider int [not null, note: 'V1 persistida: 1 Demo']
  ExternalReviewId nvarchar(200) [not null]
  Rating tinyint
  Text nvarchar(4000)
  PublishedAtUtc datetimeoffset
  AuthorDisplayName nvarchar(120)
  Language nvarchar(10)
  SourceUrl nvarchar(1000)
  RetrievedAtUtc datetimeoffset [not null]

  indexes {
    (Provider, ExternalReviewId) [unique, name: 'UX_Reviews_Provider_ExternalId']
    (BranchId, PublishedAtUtc) [name: 'IX_Reviews_Branch_Published']
    (BranchId, Rating) [name: 'IX_Reviews_Branch_Rating']
  }
}

Table ReviewCategories {
  Id uniqueidentifier [pk]
  Code nvarchar(30) [not null, unique]
  Name nvarchar(80) [not null]
  Description nvarchar(300) [not null]
  IsActive bit [not null, default: 1]
}

Table ReviewCategoryAssignments {
  ReviewId uniqueidentifier [not null]
  ReviewCategoryId uniqueidentifier [not null]
  AssignedByUserId nvarchar(450) [not null]
  AssignedAtUtc datetimeoffset [not null]

  indexes {
    (ReviewId, ReviewCategoryId) [pk, name: 'PK_ReviewCategoryAssignments']
    ReviewCategoryId [name: 'IX_ReviewCategoryAssignments_Category']
  }
}

Table ReviewCategoryAudits {
  Id uniqueidentifier [pk]
  ReviewId uniqueidentifier [not null]
  ReviewCategoryId uniqueidentifier [not null]
  Action int [not null, note: '1 Assigned; 2 Removed']
  ChangedByUserId nvarchar(450) [not null]
  ChangedAtUtc datetimeoffset [not null]

  indexes {
    (ReviewId, ChangedAtUtc) [name: 'IX_ReviewCategoryAudits_Review_Date']
  }
}

Table SimulatedDataImports {
  Id uniqueidentifier [pk]
  FileName nvarchar(255) [not null]
  RowCount int [not null]
  PeriodStart date [not null]
  PeriodEnd date [not null]
  ImportedByUserId nvarchar(450) [not null]
  ImportedAtUtc datetimeoffset [not null]
}

Table SimulatedOperationalMetrics {
  Id uniqueidentifier [pk]
  BranchId uniqueidentifier [not null]
  BusinessDate date [not null]
  NetSales decimal(18,2) [not null]
  TransactionCount int [not null]
  Currency char(3) [not null, default: 'NIO']
  DataOrigin int [not null, default: 1, note: '1 Simulated']
  ImportId uniqueidentifier [not null]

  indexes {
    (BranchId, BusinessDate) [unique, name: 'UX_SimulatedMetrics_Branch_Date']
    ImportId [name: 'IX_SimulatedMetrics_Import']
  }
}

Table IntegrationRuns {
  Id uniqueidentifier [pk]
  CorrelationId nvarchar(64) [not null, unique]
  Provider int [not null]
  BranchId uniqueidentifier [not null]
  StartedAtUtc datetimeoffset [not null]
  FinishedAtUtc datetimeoffset
  Status int [not null, note: '1 InProgress; 2 Successful; 3 Partial; 4 Failed']
  HttpStatusCode int
  RecordsReceived int [not null, default: 0]
  RecordsStored int [not null, default: 0]
  ErrorCode nvarchar(50)
  UserMessage nvarchar(500)
  TechnicalMessage nvarchar(2000)
  TriggeredByUserId nvarchar(450) [not null]

  indexes {
    (BranchId, StartedAtUtc) [name: 'IX_IntegrationRuns_Branch_Date']
    (Status, StartedAtUtc) [name: 'IX_IntegrationRuns_Status_Date']
  }
}

Table AspNetUsers {
  Id nvarchar(450) [pk]
  UserName nvarchar(256)
  NormalizedUserName nvarchar(256)
  Email nvarchar(256)
  NormalizedEmail nvarchar(256)
  PasswordHash nvarchar(max)
  SecurityStamp nvarchar(max)
  ConcurrencyStamp nvarchar(max)
  IsActive bit [not null, default: 1]
  AssignedBranchId uniqueidentifier
  CreatedAtUtc datetimeoffset [not null]

  Note: 'Resumen. Identity agrega sus columnas estándar restantes.'
}

Table AspNetRoles {
  Id nvarchar(450) [pk]
  Name nvarchar(256)
  NormalizedName nvarchar(256)
  ConcurrencyStamp nvarchar(max)
}

Table AspNetUserRoles {
  UserId nvarchar(450) [not null]
  RoleId nvarchar(450) [not null]

  indexes {
    (UserId, RoleId) [pk]
  }
}

Ref: BranchSnapshots.BranchId > Branches.Id
Ref: BranchSnapshots.IntegrationRunId > IntegrationRuns.Id
Ref: Reviews.BranchId > Branches.Id
Ref: ReviewCategoryAssignments.ReviewId > Reviews.Id
Ref: ReviewCategoryAssignments.ReviewCategoryId > ReviewCategories.Id
Ref: ReviewCategoryAssignments.AssignedByUserId > AspNetUsers.Id
Ref: ReviewCategoryAudits.ReviewId > Reviews.Id
Ref: ReviewCategoryAudits.ReviewCategoryId > ReviewCategories.Id
Ref: ReviewCategoryAudits.ChangedByUserId > AspNetUsers.Id
Ref: SimulatedDataImports.ImportedByUserId > AspNetUsers.Id
Ref: SimulatedOperationalMetrics.BranchId > Branches.Id
Ref: SimulatedOperationalMetrics.ImportId > SimulatedDataImports.Id
Ref: IntegrationRuns.BranchId > Branches.Id
Ref: IntegrationRuns.TriggeredByUserId > AspNetUsers.Id
Ref: AspNetUsers.AssignedBranchId > Branches.Id
Ref: AspNetUserRoles.UserId > AspNetUsers.Id
Ref: AspNetUserRoles.RoleId > AspNetRoles.Id
```

## 4. Restricciones adicionales para migraciones

EF Core debe expresar o agregar por SQL las siguientes restricciones:

```text
CK_BranchSnapshots_Rating: Rating IS NULL OR Rating BETWEEN 1.0 AND 5.0
CK_BranchSnapshots_ReviewCount: ReviewCount IS NULL OR ReviewCount >= 0
CK_BranchSnapshots_Latitude: Latitude IS NULL OR Latitude BETWEEN -90 AND 90
CK_BranchSnapshots_Longitude: Longitude IS NULL OR Longitude BETWEEN -180 AND 180
CK_Reviews_Rating: Rating IS NULL OR Rating BETWEEN 1 AND 5
CK_SimulatedMetrics_NetSales: NetSales >= 0
CK_SimulatedMetrics_Transactions: TransactionCount >= 0
CK_SimulatedMetrics_Origin: DataOrigin = 1
CK_Imports_Period: PeriodEnd >= PeriodStart
CK_IntegrationRuns_Counts: RecordsReceived >= 0 AND RecordsStored >= 0
```

El índice único `(Provider, ExternalPlaceId)` debe filtrar `ExternalPlaceId IS NOT NULL` en SQL Server. Solo debe existir una ejecución `InProgress` por sucursal; implementar con índice filtrado o control transaccional. Para un demo se acepta control transaccional con verificación y prueba de concurrencia.

## 5. Política de actualización y borrado

| Entidad | Crear | Actualizar | Eliminar |
|---|---|---|---|
| Branch | Administrador | Sí; código preferiblemente estable | No; inactivar |
| BranchSnapshot | Sincronización demo | No | No desde UI |
| Review | Sincronización demo/upsert | Solo metadatos del mismo ID demo | No desde UI |
| ReviewCategory | Seed/migración | Seed/migración | No; inactivar |
| Assignment | Usuario autorizado | Reemplazo de conjunto | Quitar relación + auditoría |
| Simulated metric | Importación | Reemplazo por importación explícita del mismo día | No desde UI V1 |
| IntegrationRun | Servicio | Solo finalizar | No |
| ApplicationUser | Administrador | Rol, asignación, estado | No; inactivar |

## 6. Datos semilla mínimos

| Grupo | Cantidad | Contenido |
|---|---:|---|
| Roles | 3 | `GerenteCorporativo`, `GerenteSucursal`, `Administrador` |
| Categorías | 7 | Catálogo de DOC-05 |
| Sucursales | 5 | `SUC-001` a `SUC-005`, nombres Café Horizonte |
| Usuarios | 3 mínimos | Uno por rol; credenciales solo en configuración de desarrollo |
| Snapshots | 30 mínimos | Seis fechas por sucursal |
| Reseñas | 25 mínimos | Cinco por sucursal, diversidad de calificación/categoría |
| Métricas | 90 mínimos | 18 fechas por sucursal o período equivalente |

Todos los seed de negocio son ficticios. Contraseñas no se codifican en migraciones; un inicializador de desarrollo lee valores configurados.

## 7. Consultas que debe demostrar el proyecto

| ID | Consulta | Implementación esperada |
|---|---|---|
| Q-01 | Último snapshot por sucursal | LINQ traducible a SQL; una consulta o proyección eficiente. |
| Q-02 | Variación contra snapshot anterior | Consulta/proyección; validar mismo proveedor. |
| Q-03 | Reseñas filtradas con categorías | `IQueryable`, filtros opcionales y paginación. |
| Q-04 | Conteo por categoría | `GroupBy`; explicar doble conteo. |
| Q-05 | Métricas agregadas por período | `Sum`; ticket derivado y división segura. |
| Q-06 | Bitácora filtrada | Orden descendente y paginación. |

No crear procedimientos almacenados para V1. Puede incluirse un archivo `docs/sql/demo-queries.sql` más adelante para mostrar SQL equivalente sin duplicar reglas de escritura.

## 8. Si se elige SQLite

Antes de crear la primera migración se puede sustituir SQL Server por SQLite. En ese caso:

- reemplazar `Microsoft.EntityFrameworkCore.SqlServer` por `Microsoft.EntityFrameworkCore.Sqlite`;
- adaptar tipos e índices filtrados;
- conservar exactamente las entidades, claves lógicas y pruebas;
- no mantener migraciones para ambos motores en V1.

Una vez creada la migración inicial, cambiar de motor requiere una decisión registrada en DOC-06.

## 9. Contrato para agentes de programación

```yaml
document_id: DOC-11
logical_schema: binding
physical_source_of_truth_after_implementation: ef_core_migrations
proposed_provider: Microsoft.EntityFrameworkCore.SqlServer
delete_behavior: Restrict
guid_generation: application
store_average_ticket: false
persist_google_content: false
required_queries: [Q-01, Q-02, Q-03, Q-04, Q-05, Q-06]
must_seed: [roles, categories, five_fictional_branches, demo_snapshots, demo_reviews, simulated_metrics]
```

## 10. Referencias internas

- [Modelo de dominio y diccionario](10-modelo-dominio-diccionario.md)
- [Arquitectura de la solución](12-arquitectura-solucion.md)
- [Diseño de seguridad y acceso](14-diseno-seguridad-acceso.md)

