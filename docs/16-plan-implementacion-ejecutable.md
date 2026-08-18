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

### Iteracion 11 - Dashboard analitico gerencial

Objetivo: convertir el panel en una vista de decision para gerencia, cruzando percepcion del cliente, categorias internas y metricas operativas simuladas.

Tareas:

- Replantear `/dashboard` como tablero gerencial con controles visibles.
- Agregar filtros por periodo, sucursal, categoria, calificacion/sentimiento operativo y metrica.
- Comparar sucursales en una tabla principal con:
  - calificacion;
  - cantidad de resenas;
  - categoria negativa principal;
  - ventas simuladas;
  - transacciones simuladas;
  - ticket promedio simulado;
  - nivel de atencion recomendado.
- Agregar matriz de categorias contra desempeno operativo:
  - categoria;
  - cantidad de resenas;
  - calificacion promedio de resenas categorizadas;
  - sucursales mas afectadas;
  - ventas, transacciones y ticket promedio del periodo filtrado.
- Agregar graficas con equivalente tabular:
  - dispersion calificacion vs ventas, con tamano por cantidad de resenas;
  - barras de categorias por impacto;
  - tendencia simple de ventas/transacciones si existe historial.
- Generar recomendaciones por reglas transparentes, no IA:
  - baja calificacion y bajas ventas;
  - buenas ventas con alto volumen de quejas;
  - ticket alto con quejas de precio;
  - datos faltantes o desactualizados.
- Mantener etiquetas de fuente:
  - resenas externas/demo;
  - metricas `Datos simulados`;
  - categorias `Clasificacion manual`.

Evidencia:

- El gerente corporativo puede comparar experiencia del cliente contra ventas, transacciones y ticket.
- Los filtros cambian todas las secciones relevantes.
- Cada grafica tiene tabla equivalente.
- Valores faltantes se muestran como `No disponible`.
- El panel puede explicarse en menos de dos minutos como vista ejecutiva.

Salida esperada:

- El demo muestra claramente la integracion entre sistemas y ayuda a tomar decisiones, no solo a ver tarjetas.

### Iteracion 12 - Integracion live opcional

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

### Iteracion 13 - Despliegue opcional

Objetivo: publicar cuando el demo local ya sea estable.

Tareas:

- Usar Azure App Service como plataforma inicial.
- Mantener SQLite solo para demo de una instancia usando `/home/site/data/app.db`.
- Agregar GitHub Actions para despliegue a App Service.
- Configurar variables de entorno y secretos.
- Ejecutar migraciones.
- Verificar login, dashboard, sync demo y exportacion.
- Documentar cuando migrar a Azure SQL.

Evidencia:

- URL publica carga.
- Credenciales demo funcionan.
- No hay secretos en repo.
- Workflow de GitHub Actions pasa build, pruebas y despliegue.

Salida esperada:

- Demo publicado.

## 4. Siguiente corte recomendado

El siguiente trabajo recomendado es la Iteracion 11 completa:

1. Redisenar `/dashboard` como vista analitica gerencial.
2. Cruzar resenas/categorias con ventas, transacciones y ticket.
3. Agregar controles de filtro y comparacion.
4. Mantener fuentes visibles y tablas equivalentes para graficas.
5. Ejecutar build y pruebas.

Despues de eso, evaluar si conviene avanzar a integracion live opcional o publicar primero el demo con datos controlados.

## 5. Criterios para no avanzar

Preguntar antes de continuar si:

- Una funcion requiere datos reales o nombres de una empresa real.
- Se necesita guardar contenido Google como historico.
- Se propone agregar un paquete fuera de los documentos.
- Se quiere cambiar SQLite despues de crear migraciones importantes.
- Una tarea agrega POS real, ERP real, IA, alertas o tareas.
- Se encuentra trabajo del usuario que habria que sobrescribir.
