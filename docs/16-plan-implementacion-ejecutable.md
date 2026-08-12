# Sucursal 360

## Plan de implementacion ejecutable

| Campo | Valor |
|---|---|
| Version | 0.1 |
| Fecha | 12 de agosto de 2026 |
| Estado | Plan operativo inicial |
| Objetivo | Convertir la documentacion actual en iteraciones pequenas, verificables y no bloqueadas por nube o APIs externas |

## 1. Enfoque

La primera meta es tener un demo local funcional y creible lo antes posible. Para evitar friccion
temprana, V1 usara SQLite local y datos demo controlados. Las integraciones en vivo quedan como
extensiones posteriores, no como requisito para que el producto principal funcione.

El producto se construira por cortes logicos. Cada corte debe compilar, tener una evidencia clara y
dejar el sistema en un estado usable.

## 2. Decisiones operativas actuales

| Area | Decision |
|---|---|
| Base local | SQLite |
| Base cloud futura | Azure SQL o equivalente, pendiente hasta despliegue |
| Proveedor requerido | Demo fixtures locales |
| Proveedor live opcional | Geoapify primero por baja friccion; Google Places despues si se aceptan terminos, costos y atribucion |
| Reporte | Excel con ClosedXML cuando llegue el corte de reportes |
| UI | ASP.NET Core MVC + Razor Views |
| Pruebas | MSTest |

## 3. Iteraciones

### Iteracion 0 - Preparacion del repositorio

Objetivo: dejar el proyecto consistente antes de tocar dominio.

Tareas:

- Crear `AGENTS.md`.
- Crear este plan ejecutable.
- Agregar `global.json` para fijar SDK .NET 10.
- Crear README minimo con comandos locales.
- Confirmar si se inicializara Git.

Evidencia:

- `dotnet restore Sucursal360.slnx -m:1 -nr:false`
- `dotnet build Sucursal360.slnx -m:1 -nr:false --no-restore`
- `dotnet test Sucursal360.slnx -m:1 -nr:false --no-restore --no-build`

Salida esperada:

- Base documental lista para comenzar implementacion.

### Iteracion 1 - Dominio, Identity extendido y base

Objetivo: reemplazar el scaffold vacio por el modelo real de Sucursal 360.

Tareas:

- Crear `ApplicationUser` con `IsActive`, `AssignedBranchId` y `CreatedAtUtc`.
- Crear entidades y enums de dominio:
  - `Branch`
  - `BranchSnapshot`
  - `Review`
  - `ReviewCategory`
  - `ReviewCategoryAssignment`
  - `ReviewCategoryAudit`
  - `SimulatedOperationalMetric`
  - `SimulatedDataImport`
  - `IntegrationRun`
- Configurar EF Core con restricciones, indices y relaciones.
- Crear migracion inicial de negocio para SQLite.
- Sembrar roles, categorias y cinco sucursales ficticias.

Evidencia:

- Migracion aplicada en SQLite.
- Pruebas de restricciones basicas y seed.

Salida esperada:

- La base ya representa el negocio del demo.

### Iteracion 2 - Autenticacion, roles y alcance

Objetivo: hacer que el acceso sea real y confiable.

Tareas:

- Registrar roles exactos:
  - `Administrador`
  - `GerenteCorporativo`
  - `GerenteSucursal`
- Configurar politicas de autorizacion.
- Implementar `IBranchAccessService`.
- Crear seed de usuarios de desarrollo desde configuracion o User Secrets.
- Redirigir `/` segun rol y sucursal asignada.
- Ajustar layout y navegacion por rol.

Evidencia:

- Usuario anonimo va a login.
- Gerente de sucursal no accede a otra sucursal.
- Gerente no accede a admin.

Salida esperada:

- La aplicacion ya tiene control de acceso proporcional al demo.

### Iteracion 3 - Administracion de sucursales

Objetivo: permitir mantener el catalogo demo sin eliminar historia.

Tareas:

- Crear area/rutas admin para sucursales.
- Listar, crear, editar, activar e inactivar sucursales.
- Validar codigo unico y combinacion proveedor/identificador externo.
- Evitar borrado fisico desde UI.

Evidencia:

- Admin puede mantener sucursales.
- Usuarios no admin reciben 403.

Salida esperada:

- El catalogo de cinco sucursales puede administrarse desde la app.

### Iteracion 4 - Proveedor demo y sincronizacion

Objetivo: demostrar integracion sin depender de internet.

Tareas:

- Crear DTOs canonicos `ExternalBranchData`, `ExternalReviewData` y `ExternalAttribution`.
- Crear `IPublicBranchDataProvider`.
- Implementar `DemoPublicBranchDataProvider`.
- Crear fixtures JSON para cinco sucursales.
- Implementar validacion canonica.
- Implementar `IBranchSynchronizationService`.
- Persistir snapshots y reviews del proveedor Demo.
- Registrar `IntegrationRun` con estado, cantidades, mensaje seguro y correlacion.
- Crear UI admin para sincronizacion individual y general.

Evidencia:

- Cinco sucursales sincronizan sin red.
- Un fixture parcial produce estado parcial.
- Un error conserva datos previos.

Salida esperada:

- El demo ya muestra una integracion completa y repetible.

### Iteracion 5 - Dashboard corporativo

Objetivo: construir la primera pantalla de valor gerencial.

Tareas:

- Crear ruta `/dashboard`.
- Consultar ultimo snapshot por sucursal.
- Calcular variacion contra snapshot anterior.
- Mostrar filtros basicos, tabla comparativa y estados.
- Mostrar resumen operativo simulado separado.
- Renderizar `No disponible` para valores faltantes.

Evidencia:

- Gerente corporativo y admin ven todas las sucursales.
- Gerente de sucursal no entra al dashboard corporativo.
- Cada metrica muestra fuente y fecha.

Salida esperada:

- La aplicacion deja de parecer scaffold y empieza a vender el concepto.

### Iteracion 6 - Detalle de sucursal e historico

Objetivo: permitir profundizar en una sucursal autorizada.

Tareas:

- Crear ruta `/branches/{id}`.
- Mostrar datos generales, ultimo snapshot, historial y ultima sincronizacion.
- Agregar una grafica simple con Chart.js y tabla equivalente.
- Mostrar metricas operativas simuladas en seccion separada.

Evidencia:

- Gerente de sucursal ve solo su sucursal.
- Valores sin datos aparecen como `No disponible`.
- La grafica tiene tabla equivalente.

Salida esperada:

- El demo permite explicar tendencias, no solo estado actual.

### Iteracion 7 - Resenas y clasificacion manual

Objetivo: demostrar analisis gerencial de comentarios.

Tareas:

- Crear ruta `/reviews`.
- Filtrar por sucursal, fecha, calificacion y categoria.
- Mostrar conteos por categoria.
- Implementar `IReviewCategorizationService`.
- Reemplazar categorias de una resena en una transaccion.
- Registrar auditoria de asignar/quitar categoria.

Evidencia:

- Una resena puede tener varias categorias.
- Quitar una categoria queda auditado.
- El texto original de la resena no cambia.

Salida esperada:

- El demo muestra analisis manual, controlado y auditable.

### Iteracion 8 - Importacion CSV simulada

Objetivo: representar una futura integracion POS/ERP sin conectarse a un POS real.

Tareas:

- Crear ruta `/admin/simulated-data/import`.
- Validar archivo CSV UTF-8, encabezados y filas.
- Rechazar importacion completa si una fila es invalida.
- Persistir `SimulatedDataImport` y `SimulatedOperationalMetric`.
- Mantener etiqueta `Datos simulados`.

Evidencia:

- CSV valido importa todo.
- CSV con una fila invalida guarda cero filas.
- Ticket promedio se calcula, no se persiste.

Salida esperada:

- El dashboard puede mezclar reputacion publica demo con operacion simulada claramente identificada.

### Iteracion 9 - Reporte Excel

Objetivo: entregar una salida gerencial reutilizable.

Tareas:

- Agregar ClosedXML.
- Crear `IManagementReportExporter`.
- Crear rutas `/reports/management` y `/reports/management/export`.
- Generar cinco hojas:
  - `Resumen`
  - `Sucursales`
  - `Tendencias`
  - `Categorias`
  - `Operacion_Simulada`
- Neutralizar valores peligrosos para Excel.

Evidencia:

- El archivo abre correctamente.
- Contiene filtros, fuentes, fecha de corte y `Datos simulados`.
- Texto con prefijo de formula queda neutralizado.

Salida esperada:

- El demo puede cerrarse con un entregable ejecutivo.

### Iteracion 10 - Pulido del demo local

Objetivo: preparar una demo de 5 a 7 minutos.

Tareas:

- Revisar textos de UI en espanol.
- Mejorar estados vacios, parciales y fallidos.
- Revisar accesibilidad basica.
- Completar README.
- Agregar guion corto de demo.
- Revisar que no haya secretos ni datos reales.

Evidencia:

- Build y pruebas pasan.
- Una persona puede ejecutar el proyecto siguiendo README.

Salida esperada:

- Demo local listo y presentable.

### Iteracion 11 - Integracion live opcional

Objetivo: agregar una prueba de integracion real sin poner en riesgo el demo base.

Tareas:

- Elegir un proveedor live:
  - Geoapify para baja friccion.
  - Google Places para historia comercial mas fuerte, si se aceptan terminos, billing y atribucion.
- Implementar otro `IPublicBranchDataProvider`.
- Configurar secretos fuera del repo.
- Mostrar datos live como efimeros o con la politica permitida por el proveedor.
- Mantener Demo provider como fallback.

Evidencia:

- La app funciona sin API key.
- Con API key, una sucursal puede consultar datos live.
- La bitacora registra exito/fallo.

Salida esperada:

- El demo tiene una historia de integracion real sin depender de ella para funcionar.

### Iteracion 12 - Despliegue opcional

Objetivo: publicar cuando el demo local ya sea estable.

Tareas:

- Elegir plataforma.
- Si se usa Azure App Service, mover base de datos a Azure SQL u otra base administrada.
- Configurar variables de entorno y secretos.
- Ejecutar migraciones.
- Verificar login, dashboard, sync demo y exportacion.

Evidencia:

- URL publica carga.
- Credenciales demo funcionan.
- No hay secretos en repo.

Salida esperada:

- Demo publicado.

## 4. Primer corte recomendado

El siguiente trabajo recomendado es la Iteracion 0 completa:

1. Agregar `global.json`.
2. Crear README minimo.
3. Confirmar si se inicializara Git.
4. Ejecutar restore, build y test.

Despues de eso, comenzar la Iteracion 1 con el modelo de dominio y la migracion inicial.

## 5. Criterios para no avanzar

Preguntar antes de continuar si:

- Una funcion requiere datos reales o nombres de una empresa real.
- Se necesita guardar contenido Google como historico.
- Se propone agregar un paquete fuera de los documentos.
- Se quiere cambiar SQLite despues de crear migraciones importantes.
- Una tarea agrega POS real, ERP real, IA, alertas o tareas.
- Se encuentra trabajo del usuario que habria que sobrescribir.
