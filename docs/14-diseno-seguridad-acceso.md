# Sucursal 360

## Diseño de seguridad y acceso

| Campo | Valor |
|---|---|
| Versión | 1.0 |
| Fecha | 12 de agosto de 2026 |
| Estado | Línea base de seguridad proporcional al demo |
| Tecnología | ASP.NET Core Identity y autorización MVC |

## 1. Objetivo y proporción

El demo debe evitar fallos básicos que resten credibilidad: acceso sin autorización, filtración entre sucursales, secretos en el repositorio, CSRF, entradas no validadas y logs sensibles. No pretende sustituir un diseño empresarial de identidad, cumplimiento o monitoreo.

No se implementan SSO, MFA, Microsoft Entra ID, Key Vault, WAF, SIEM, rotación automática, gestión de consentimiento ni pruebas de penetración formales.

## 2. Activos y amenazas principales

| ID | Activo | Amenaza | Control V1 |
|---|---|---|---|
| TH-01 | Datos por sucursal | Gerente consulta otra sucursal cambiando URL | Política de recurso en servidor. |
| TH-02 | Funciones admin | Usuario invoca POST manual | `[Authorize]` + política/rol + antiforgery. |
| TH-03 | Clave Google/conexión | Secreto en Git o log | User Secrets/App Settings; redacción. |
| TH-04 | Formularios/CSV | Entrada maliciosa o excesiva | ViewModels, límites, validación y encoding Razor. |
| TH-05 | Excel | Inyección de fórmula | Neutralizar prefijos peligrosos. |
| TH-06 | Cuenta demo | Fuerza bruta | Lockout y mensaje genérico. |
| TH-07 | Bitácora | Payload o excepción expone datos | Mensaje sanitizado + correlación. |

## 3. Autenticación

Usar ASP.NET Core Identity con cookies y almacenamiento EF Core. Identity soporta usuarios, contraseñas, roles, tokens y bloqueo; fuente: [documentación oficial](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-10.0).

### Configuración inicial

```text
RequireUniqueEmail = true
Password.RequiredLength = 10
Password.RequireUppercase = true
Password.RequireLowercase = true
Password.RequireDigit = true
Password.RequireNonAlphanumeric = false
Lockout.MaxFailedAccessAttempts = 5
Lockout.DefaultLockoutTimeSpan = 15 minutes
Cookie.HttpOnly = true
Cookie.SecurePolicy = Always outside Development
Cookie.SameSite = Lax
ExpireTimeSpan = 8 hours
SlidingExpiration = true
```

No hay autorregistro. El Administrador crea usuarios. En desarrollo, las credenciales semilla provienen de User Secrets o variables de entorno y el README explica cómo establecerlas. Nunca incluir contraseñas funcionales en Markdown, seed, captura o Git.

`ApplicationUser.IsActive` se valida en login y en cada solicitud autenticada mediante un control adecuado; al inactivar, actualizar `SecurityStamp` para invalidar sesiones.

## 4. Roles y políticas

Nombres exactos y sensibles a mayúsculas:

```csharp
public static class AppRoles
{
    public const string CorporateManager = "GerenteCorporativo";
    public const string BranchManager = "GerenteSucursal";
    public const string Administrator = "Administrador";
}
```

Registrar roles mediante `AddRoles<IdentityRole>()`. ASP.NET Core permite controles declarativos y políticas basadas en roles; fuente: [autorización por roles](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-10.0).

| Política | Regla |
|---|---|
| `CanViewCorporateDashboard` | `GerenteCorporativo` o `Administrador` |
| `CanExportManagementReport` | `GerenteCorporativo` o `Administrador` |
| `CanAdministerSystem` | `Administrador` |
| `CanViewBranch` | Rol corporativo/admin o `AssignedBranchId == resource.BranchId` |
| `CanCategorizeReview` | Igual a `CanViewBranch` y reseña persistida DEMO |

La navegación usa roles para mostrar enlaces, pero la autorización definitiva ocurre en controladores/servicios.

## 5. Matriz endpoint–política

| Endpoint | Método | Política/rol |
|---|---|---|
| `/dashboard` | GET | `CanViewCorporateDashboard` |
| `/branches/{id}` | GET | Autenticado + `CanViewBranch` |
| `/reviews` | GET | Autenticado; consulta limitada por alcance |
| `/reviews/{id}/categories` | POST | `CanCategorizeReview` |
| `/reports/management` | GET/POST | `CanExportManagementReport` |
| `/admin/branches/*` | Todos | `CanAdministerSystem` |
| `/admin/integrations/*` | Todos | `CanAdministerSystem` |
| `/admin/simulated-data/*` | Todos | `CanAdministerSystem` |
| `/admin/users/*` | Todos | `CanAdministerSystem` |
| `/legal/*` | GET | Anónimo |

## 6. Alcance por sucursal

```csharp
public interface IBranchAccessService
{
    Task<bool> CanAccessAsync(
        ClaimsPrincipal user,
        Guid branchId,
        CancellationToken cancellationToken);
}
```

Reglas:

1. Administrador y GerenteCorporativo acceden a todas las sucursales.
2. GerenteSucursal accede únicamente a `ApplicationUser.AssignedBranchId`.
3. Sin asignación, acceso denegado; no elegir la primera sucursal.
4. Todas las consultas de reseñas, métricas e histórico comienzan por el conjunto autorizado.
5. Respuesta fuera de alcance: 403. No redirigir a otra sucursal silenciosamente.

## 7. Formularios, entradas y salida

- Todos los POST MVC usan token antiforgery; configurar filtro global `AutoValidateAntiforgeryToken`.
- Usar ViewModels con atributos de validación y listas permitidas para enums.
- Razor codifica salida por defecto; no usar `Html.Raw` con contenido externo.
- Limitar CSV a 2 MB y extensiones/MIME esperados, validando contenido real.
- Normalizar código de sucursal y nombre de archivo; nunca combinar entrada con rutas.
- Validar URLs externas con `Uri.TryCreate`, esquemas `https` y proveedor esperado.
- Neutralizar celdas Excel que comiencen con `=`, `+`, `-`, `@`, tab o retorno.

## 8. Secretos y configuración

| Ambiente | Mecanismo |
|---|---|
| Local | `dotnet user-secrets` |
| Azure App Service | Application Settings / Connection strings |
| Pruebas | Valores falsos en memoria; ninguna clave real |

Secretos: conexión SQL, clave Google opcional y credenciales de inicialización. `.gitignore` debe excluir archivos locales de secretos. No se requiere Azure Key Vault para este demo.

## 9. Manejo de errores y logs

- Middleware global asigna/propaga `CorrelationId`.
- Usuario recibe código funcional y correlación; nunca stack trace.
- Producción/demo publicado usa `/Error` y HSTS/HTTPS.
- `ILogger` usa plantillas estructuradas, por ejemplo `Branch sync failed {BranchId} {ErrorCode} {CorrelationId}`.
- No registrar contraseñas, cookies, connection strings, API keys, headers completos, cuerpos Google ni texto completo de reseñas.
- `TechnicalMessage` guarda tipo/resumen sanitizado, no `Exception.ToString()` completo.

## 10. Auditoría mínima

| Evento | Registro |
|---|---|
| Sincronización | `IntegrationRun`, usuario, tiempos, resultado |
| Asignar/quitar categoría | `ReviewCategoryAudit` |
| Importación CSV | `SimulatedDataImport` |
| Crear/inactivar usuario o sucursal | Log estructurado con actor e ID; no se requiere tabla genérica |
| Login fallido/bloqueado | Logging de Identity sin contraseña |

## 11. Seguridad de Azure gratuita

- Forzar HTTPS y no publicar puertos de base de datos innecesarios.
- La cadena Azure SQL está solo en App Service.
- Restringir el firewall de Azure SQL al servicio y dirección administrativa necesaria; no abrir `0.0.0.0/0` como solución permanente.
- Elegir pausa al alcanzar límites gratuitos para evitar cargos.
- No guardar la clave Google en configuración de aplicación versionada.
- Las cuentas demo se inactivan o se eliminan cuando la demostración pública termina.

## 12. Páginas legales para Google

Si `PublicData:Provider = GooglePlaces`, deben existir páginas públicas de Términos y Privacidad, la atribución requerida y enlaces de autor/origen. Si faltan, la aplicación debe rechazar el arranque del proveedor Google o mantenerlo deshabilitado. El modo DEMO no debe mostrar atribución de Google.

## 13. Pruebas de seguridad obligatorias

| ID | Prueba |
|---|---|
| SEC-T01 | Anónimo es redirigido a login en rutas privadas. |
| SEC-T02 | GerenteSucursal recibe 403 para otra sucursal. |
| SEC-T03 | Gerente no puede invocar endpoints admin por POST. |
| SEC-T04 | POST sin antiforgery es rechazado. |
| SEC-T05 | Usuario inactivo no inicia y sesión previa queda invalidada. |
| SEC-T06 | Cinco fallos producen lockout según configuración. |
| SEC-T07 | CSV con nombre/ruta maliciosa no escribe archivos. |
| SEC-T08 | Texto de reseña se codifica en HTML. |
| SEC-T09 | Fórmula potencial en Excel se neutraliza. |
| SEC-T10 | Claves/cadena no aparecen en logs ni repositorio. |

## 14. Contrato para agentes de programación

```yaml
document_id: DOC-14
authentication: ASP.NET_Core_Identity_cookie
self_registration: false
mfa: out_of_scope
sso: out_of_scope
roles: [GerenteCorporativo, GerenteSucursal, Administrador]
branch_scope_enforcement: server_side_required
csrf: global_auto_validate
local_secrets: dotnet_user_secrets
azure_secrets: app_service_settings
key_vault: not_required
never_log:
  - passwords
  - cookies
  - api_keys
  - connection_strings
  - external_response_bodies
  - full_review_text
```

## 15. Referencias internas

- [Roles y requisitos](04-requisitos-negocio.md)
- [Procesos y casos de uso](08-procesos-casos-uso.md)
- [Diseño de integraciones](13-diseno-integraciones-contratos.md)
- [Plan de implementación](15-plan-implementacion-ia.md)

