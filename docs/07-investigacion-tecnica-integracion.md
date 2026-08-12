# Sucursal 360

## Investigación técnica de la integración

| Campo | Valor |
|---|---|
| Versión | 1.0 |
| Fecha | 12 de agosto de 2026 |
| Estado | Línea base de diseño |
| Decisiones relacionadas | D-12 a D-15; P-02, P-03, P-05 y P-07 |

## 1. Resultado ejecutivo

La integración pública es viable para el demo si se implementa mediante una interfaz intercambiable y se separan dos modos de ejecución:

1. **Modo demo reproducible:** `DemoPublicDataProvider`, alimentado con archivos JSON sintéticos incluidos en el repositorio. Permite crear históricos, reseñas ficticias y escenarios de error sin depender de red, costo o licencias.
2. **Modo Google en vivo opcional:** `GooglePlacesProvider`, utilizado únicamente con una cuenta autorizada, atribución visible y tratamiento de datos compatible con los términos vigentes. No será requisito para compilar, ejecutar pruebas ni presentar el recorrido principal.

Google Places es técnicamente adecuado para mostrar una llamada externa real, pero no se aprueba como fuente persistente del histórico del demo. La política oficial prohíbe almacenar contenido de Places salvo excepciones; el identificador del lugar sí puede conservarse indefinidamente. Por tanto, calificaciones, horarios y reseñas históricas persistentes del recorrido principal serán sintéticos. La aplicación nunca presentará esos datos como obtenidos de Google.

## 2. Decisiones resultantes

| ID | Decisión de diseño | Estado | Consecuencia para implementación |
|---|---|---|---|
| DT-01 | Usar .NET 10 LTS | Bloqueada | Todos los proyectos usarán `net10.0`. |
| DT-02 | Usar ASP.NET Core MVC con Razor Views | Bloqueada | Aplicación web renderizada en servidor; no SPA. |
| DT-03 | Usar SQL Server 2022 Developer o Express con EF Core | Propuesta recomendada | Refuerza la demostración de .NET, consultas SQL y reportes; cambiar a SQLite sigue siendo posible antes de programar. |
| DT-04 | Usar ASP.NET Core Identity con roles | Bloqueada | Autenticación local y los tres roles definidos. |
| DT-05 | Usar una aplicación MVC y un proyecto de pruebas | Bloqueada | Separación por carpetas e interfaces dentro de `Sucursal360.Web`; no microservicios ni múltiples capas físicas. |
| DT-06 | Mantener todo proveedor público detrás de `IPublicBranchDataProvider` | Bloqueada | Ningún controlador conoce DTO de Google ni lee JSON directamente. |
| DT-07 | Hacer obligatorio el proveedor sintético y opcional Google Places | Bloqueada | CI y demo principal funcionan sin clave externa. |
| DT-08 | Usar ClosedXML para Excel, sujeto a verificación final de licencia al instalar | Propuesta recomendada | Excepción pragmática: es externa a Microsoft, pero reduce mucho el código frente a Open XML SDK. Encapsular tras `IManagementReportExporter`. |
| DT-09 | Fijar calificación baja en `<= 2` para la primera versión | Bloqueada | Configuración `Reviews:LowRatingMaximum = 2`. |
| DT-10 | Usar Azure App Service F1 + Azure SQL Database Free para la publicación del demo | Propuesta recomendada | Mantiene el recorrido Microsoft; configurar pausa al alcanzar el límite para evitar cargos. Ejecución local sigue siendo obligatoria. |

## 3. Investigación de Google Places API

### 3.1 Operación candidata

Place Details (New) usa una solicitud `GET` a:

```http
GET https://places.googleapis.com/v1/places/{placeId}
X-Goog-Api-Key: {apiKey}
X-Goog-FieldMask: id,displayName,formattedAddress,businessStatus,rating,userRatingCount,regularOpeningHours,googleMapsUri,reviews
Accept-Language: es-NI
```

El `FieldMask` es obligatorio y determina tanto la respuesta como la categoría de facturación. La máscara anterior es ilustrativa; el modo publicado debe construirla desde una constante revisada y nunca usar `*`.

Fuente: [Place Details (New), documentación oficial](https://developers.google.com/maps/documentation/places/web-service/place-details).

### 3.2 Campos y costo relativo

| Campo canónico | Campo Google | Nivel de facturación observado | Uso permitido en Sucursal 360 |
|---|---|---|---|
| `ExternalPlaceId` | `id` | IDs Only | Persistir como configuración. |
| `DisplayName` | `displayName.text` | Pro | Mostrar en vista en vivo con atribución. |
| `Address` | `formattedAddress` | Essentials | Mostrar en vista en vivo con atribución. |
| `BusinessStatus` | `businessStatus` | Pro | Mostrar en vista en vivo. |
| `Rating` | `rating` | Enterprise | Mostrar en vivo; no persistir por defecto. |
| `ReviewCount` | `userRatingCount` | Enterprise | Mostrar en vivo; no persistir por defecto. |
| `OpeningHoursText` | `regularOpeningHours.weekdayDescriptions` | Enterprise | Mostrar en vivo; no persistir por defecto. |
| `SourceUrl` | `googleMapsUri` | Pro | Enlace visible al origen. |
| `Reviews` | `reviews[]` | Enterprise + Atmosphere | Solo vista en vivo si términos y atribución se cumplen. |

A la fecha de esta investigación, el nivel Enterprise + Atmosphere tiene un cupo mensual gratuito limitado y cobro por cada mil solicitudes después del cupo. Los importes pueden cambiar; no deben codificarse. Consultar [precios oficiales](https://developers.google.com/maps/billing-and-pricing/pricing) antes de habilitar el proveedor.

### 3.3 Restricciones que gobiernan el diseño

| Restricción | Regla del sistema |
|---|---|
| Contenido de Places no debe preconsultarse, almacenarse o cachearse fuera de las excepciones aplicables | `GooglePlacesProvider` devuelve un DTO efímero. El servicio de sincronización no crea `BranchSnapshot` ni `Review` a partir de Google en la configuración inicial. |
| El `place_id` está exceptuado de la restricción de cache | Se permite guardar `Branch.ExternalPlaceId`; debe poder renovarse si queda obsoleto. |
| Datos sin mapa requieren atribución de Google | Toda vista en vivo incluye el logo/atribución y los enlaces exigidos. |
| Reseñas requieren atribución del autor y enlaces disponibles | La vista no elimina ni reemplaza la atribución recibida. |
| La aplicación debe publicar términos de uso y política de privacidad | Habilitar Google exige páginas `/legal/terminos` y `/legal/privacidad`. |

Fuente: [Políticas y atribuciones de Places API](https://developers.google.com/maps/documentation/places/web-service/policies) y [guía oficial de Place IDs](https://developers.google.com/maps/documentation/places/web-service/place-id).

### 3.4 Veredicto del proveedor

| Criterio | Resultado | Veredicto |
|---|---|---|
| Cobertura técnica de detalle y calificación | Los campos existen | Apto |
| Reseñas completas para análisis gerencial | La API entrega una selección, no un corpus garantizado | Limitado |
| Histórico persistente | Las restricciones de almacenamiento impiden asumirlo | No aprobado |
| Costo para demo ocasional | Probablemente bajo, pero requiere cuenta de facturación y control de máscara | Condicional |
| Funcionamiento sin red | No | Requiere alternativa |
| Demostración de integración real | Sí | Apto como recorrido opcional |

**Conclusión:** Google Places queda como adaptador opcional de consulta en vivo. `DemoPublicDataProvider` es la fuente vinculante para históricos, clasificación manual, pruebas y guion de demostración.

## 4. Contrato del proveedor sintético

Los fixtures viven en `src/Sucursal360.Infrastructure/Providers/Demo/Fixtures/` y son datos inventados. Un fixture por sucursal sigue este esquema:

```json
{
  "schemaVersion": "1.0",
  "provider": "DEMO",
  "externalPlaceId": "DEMO-SUC-001",
  "displayName": "Café Horizonte Centro",
  "address": "Dirección ficticia 1, Managua",
  "latitude": 12.1364,
  "longitude": -86.2514,
  "businessStatus": "OPERATIONAL",
  "openingHoursText": ["Lunes a domingo: 07:00-21:00"],
  "rating": 4.3,
  "reviewCount": 128,
  "retrievedAtUtc": "2026-08-01T14:00:00Z",
  "reviews": [
    {
      "externalReviewId": "DEMO-REV-001",
      "rating": 2,
      "text": "La espera fue más larga de lo esperado.",
      "publishedAtUtc": "2026-07-28T18:30:00Z",
      "authorDisplayName": "Cliente demo 01",
      "language": "es",
      "sourceUrl": null
    }
  ]
}
```

Reglas:

- `schemaVersion`, `provider`, `externalPlaceId` y `retrievedAtUtc` son obligatorios.
- `provider` debe ser `DEMO`.
- `rating` admite `null` o un decimal entre 1 y 5.
- `reviewCount` admite `null` o entero no negativo.
- `externalReviewId` es único dentro del proveedor.
- Los nombres, direcciones, textos y autores deben ser ficticios.
- El fixture se deserializa y valida con las mismas reglas canónicas del proveedor real.

## 5. Decisiones de plataforma .NET

### 5.1 Versión y soporte

.NET 10 es LTS, está en soporte activo y su fin de soporte oficial está previsto para noviembre de 2028. El repositorio debe fijar el SDK mediante `global.json` y actualizar únicamente parches `10.0.x` compatibles. Fuente: [política oficial de soporte de .NET](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core).

### 5.2 Aplicación y persistencia

| Componente | Selección | Razón |
|---|---|---|
| UI web | ASP.NET Core MVC + Razor Views + Bootstrap 5 | Reaprendizaje directo de MVC, formularios, validación y autorización; alcance pequeño. |
| ORM | EF Core 10 | Migraciones, consultas LINQ y restricciones declarativas. |
| Base de datos | SQL Server 2022 Developer/Express | Alineación con Microsoft y demostración de SQL. SQLite es la alternativa de menor instalación. |
| Autenticación | ASP.NET Core Identity | Manejo integrado de usuarios, contraseñas, cookies y roles. |
| Cliente HTTP | `IHttpClientFactory` + `Microsoft.Extensions.Http.Resilience` | Cliente tipado, timeout y reintentos controlados para operaciones GET. |
| Reporte | ClosedXML detrás de interfaz | Generación de `.xlsx` sin depender de Office. |
| Gráficas | Chart.js | Gráficas pequeñas en el navegador desde modelos ya autorizados. |
| Pruebas | MSTest + `WebApplicationFactory` | Opción Microsoft para pruebas unitarias e integración HTTP. |
| Logging | `ILogger` estructurado | Suficiente para el demo; sin infraestructura adicional obligatoria. |

EF Core Migrations mantiene el esquema sincronizado con el modelo y conserva historial de migraciones versionable; consultar la [guía oficial](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/). ASP.NET Core Identity gestiona usuarios, contraseñas, roles y tokens; consultar la [introducción oficial](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-10.0).

### 5.3 Resiliencia HTTP

Para `GET` del proveedor externo:

- timeout total: 15 segundos;
- máximo 2 reintentos;
- espera exponencial con jitter;
- reintentar `408`, `429`, `5xx`, `HttpRequestException` y timeout transitorio;
- respetar `Retry-After` cuando exista;
- no reintentar `400`, `401`, `403` ni `404`;
- nunca registrar la clave ni el cuerpo completo de una respuesta con datos personales;
- propagar `X-Correlation-ID` internamente y guardarlo en `IntegrationRun`.

.NET ofrece manejadores estándar de resiliencia con limitación, timeout, reintento y circuit breaker. Para este demo se configurará una política más pequeña y explícita. Fuente: [patrones oficiales de resiliencia HTTP](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience).

## 6. Configuración y secretos

```text
PublicData__Provider=Demo | GooglePlaces
PublicData__Demo__FixturesPath=Providers/Demo/Fixtures
PublicData__GooglePlaces__BaseUrl=https://places.googleapis.com/v1/
PublicData__GooglePlaces__ApiKey=<secret>
PublicData__GooglePlaces__LanguageCode=es
PublicData__GooglePlaces__RegionCode=NI
Reviews__LowRatingMaximum=2
ConnectionStrings__DefaultConnection=<secret>
Display__TimeZoneId=America/Managua
```

`appsettings.json` contiene valores no sensibles. Secret Manager se usa localmente y variables de entorno o un almacén de secretos en despliegue. Ningún fixture, log, captura o prueba contiene una clave real.

## 7. Spike técnico pendiente

El diseño documental no sustituye una llamada autorizada. Antes de activar Google se debe ejecutar `SPIKE-INT-01`:

1. Crear un proyecto y clave con restricciones de API.
2. Definir presupuesto y alertas de facturación.
3. Consultar un establecimiento autorizado con una máscara mínima sin `reviews`.
4. Verificar cobertura de Nicaragua, idioma, errores y latencia.
5. Repetir con `reviews` solo si el costo y los términos fueron aceptados.
6. Confirmar atribución visual con la documentación vigente.
7. Registrar capturas sin clave ni datos innecesarios.
8. Actualizar P-02 y registrar el resultado del spike; no modificar automáticamente el modo demo.

### Criterio de aprobación

`SPIKE-INT-01` se aprueba si la llamada devuelve los campos esperados, el costo está controlado, la presentación cumple atribución y no se requiere persistir contenido prohibido. Si falla, Google permanece deshabilitado y el producto sigue completo con el proveedor demo.

## 8. Contrato para agentes de programación

```yaml
document_id: DOC-07
binding_decisions:
  target_framework: net10.0
  web_model: aspnet_core_mvc_razor
  database: sql_server_proposed
  orm: ef_core
  authentication: aspnet_core_identity
  required_public_provider: DEMO
  optional_public_provider: GOOGLE_PLACES_LIVE
  low_rating_maximum: 2
invariants:
  - CI_MUST_NOT_REQUIRE_EXTERNAL_API_KEY
  - GOOGLE_CONTENT_MUST_NOT_CREATE_PERSISTED_SNAPSHOTS_OR_REVIEWS_BY_DEFAULT
  - DEMO_DATA_MUST_ALWAYS_BE_LABELED_AS_SYNTHETIC
  - CONTROLLERS_MUST_DEPEND_ON_APPLICATION_INTERFACES_NOT_PROVIDER_DTOS
forbidden:
  - web_scraping
  - wildcard_google_field_mask_outside_explicit_spike
  - secrets_in_repository_or_logs
  - automatic_sentiment_or_review_classification
stop_conditions:
  - provider_terms_are_unclear
  - a_required_field_needs_prohibited_storage
  - implementation_requires_real_pos_or_erp_access
```

## 9. Referencias internas

- [Requisitos de negocio y del sistema](04-requisitos-negocio.md)
- [Integraciones, datos, KPI y reportes](05-integraciones-kpi-datos.md)
- [Decisiones, supuestos y riesgos](06-decisiones-supuestos-riesgos.md)
- [Diseño de integraciones y contratos](13-diseno-integraciones-contratos.md)
- [Diseño de seguridad y acceso](14-diseno-seguridad-acceso.md)
