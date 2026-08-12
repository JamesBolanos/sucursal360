# Sucursal 360

## Alcance, objetivos y criterios de éxito

| Campo | Valor |
|---|---|
| Versión | 0.1 |
| Fecha | 12 de agosto de 2026 |
| Estado | Línea base inicial |
| Documento relacionado | [Ficha de iniciación](01-ficha-iniciacion-demo.md) |

## 1. Propósito

Definir los límites del demo Sucursal 360 y establecer qué debe entregar la primera versión. Este documento es la referencia principal para evitar que el ejercicio crezca hasta convertirse en un POS, CRM o sistema completo de gestión de restaurantes.

## 2. Objetivo general

Desarrollar una aplicación web en .NET que consolide datos públicos de varias sucursales y los convierta en indicadores gerenciales de reputación, tendencias y temas de atención, complementados con una muestra claramente identificada de datos operativos simulados.

## 3. Objetivos específicos

1. Mantener un catálogo de cinco sucursales de demostración con su identificador interno y su referencia en el proveedor externo.
2. Consultar y almacenar información pública disponible mediante una integración autorizada.
3. Conservar instantáneas históricas de calificación y cantidad de reseñas para mostrar tendencias.
4. Presentar un panel que permita comparar sucursales y profundizar en el detalle de cada local.
5. Organizar las reseñas mediante categorías gerenciales predefinidas.
6. Incorporar ventas, transacciones y ticket promedio simulados sin presentarlos como información real.
7. Generar un reporte gerencial filtrable y exportable a Excel.
8. Registrar el resultado de cada sincronización para facilitar soporte y diagnóstico.

## 4. Alcance incluido

### 4.1 Administración básica

- Inicio de sesión local.
- Roles: gerente corporativo, gerente de sucursal y administrador.
- Catálogo de sucursales.
- Activación e inactivación de sucursales.
- Asociación entre sucursal e identificador del proveedor externo.

### 4.2 Integración pública

- Consulta manual de datos públicos por sucursal.
- Sincronización general iniciada por el administrador.
- Obtención de los campos que el proveedor autorizado exponga y permita almacenar.
- Registro de fecha, estado, duración y mensaje de la sincronización.
- Manejo controlado de respuestas incompletas, límites y fallos del proveedor.

### 4.3 Información y análisis

- Resumen corporativo de sucursales.
- Calificación actual y cantidad de reseñas.
- Variación respecto de la instantánea anterior.
- Detalle por sucursal.
- Listado de reseñas disponibles con filtros.
- Categorización manual de reseñas.
- Categorías iniciales: servicio, tiempo de espera, calidad del producto, limpieza, precio, instalaciones y otros.
- Historial de indicadores por sucursal.

### 4.4 Datos operativos simulados

- Dataset controlado de ventas netas, transacciones y ticket promedio.
- Importación desde un archivo CSV de demostración o carga inicial de datos semilla.
- Etiqueta visible **Datos simulados** en todas las pantallas y exportaciones correspondientes.
- Uso ilustrativo; no se calcularán conclusiones causales entre ventas y reseñas.

### 4.5 Reporte

- Reporte gerencial por período.
- Comparación de sucursales.
- Indicadores públicos y operativos simulados.
- Filtros por sucursal, rango de fechas, calificación y categoría.
- Exportación a Excel.

## 5. Fuera de alcance

- Operación de caja o registro de órdenes.
- Procesamiento de pagos.
- Facturación fiscal o electrónica.
- Inventario, recetas, cocina o compras.
- Integración real con el POS o ERP de una empresa.
- Escritura o modificación de información en plataformas públicas.
- Respuesta a reseñas desde la aplicación.
- Alertas por correo, mensajería o creación automática de tareas.
- Flujo formal de gestión y cierre de quejas.
- Clasificación automática con inteligencia artificial.
- Análisis avanzado de sentimiento.
- Aplicación móvil nativa.
- Alta disponibilidad, escalamiento empresarial o soporte 24/7.

## 6. Entregables

| Entregable | Contenido mínimo |
|---|---|
| Línea base de iniciación y análisis | Los seis documentos Markdown de esta fase |
| Prototipo de experiencia | Wireframes de panel, comparación, detalle y reseñas |
| Diseño de solución | Arquitectura, modelo de datos y contrato de integración |
| Aplicación web | Funciones incluidas en el alcance |
| Base de datos | Esquema, migraciones y datos semilla |
| Pruebas | Pruebas unitarias, integración y aceptación priorizadas |
| Reporte | Vista gerencial y exportación a Excel |
| Paquete de demostración | README, instrucciones de ejecución y guion breve de demo |

## 7. Criterios de éxito

El demo se considerará exitoso cuando:

- un usuario pueda consultar cinco sucursales desde un único panel;
- la aplicación consuma al menos un servicio externo real o un entorno sandbox oficialmente permitido;
- se almacene el resultado y la trazabilidad de las sincronizaciones;
- sea posible comparar calificación, cantidad de reseñas y tendencia entre sucursales;
- las reseñas puedan filtrarse y asignarse a categorías gerenciales;
- los indicadores simulados estén visualmente diferenciados de la información pública;
- el reporte gerencial se exporte correctamente a Excel;
- la aplicación continúe disponible cuando el proveedor externo falle y comunique el último dato exitoso;
- los principales requisitos cuenten con evidencia de prueba;
- otra persona pueda ejecutar el proyecto siguiendo el README.

## 8. Cronograma de referencia

Estimación para trabajo individual a tiempo parcial. Se considera un supuesto de planificación, no un compromiso contractual.

| Semana | Resultado esperado |
|---|---|
| 1 | Línea base, wireframes, arquitectura, modelo de datos y prueba técnica del proveedor externo |
| 2 | Proyecto .NET, autenticación, sucursales, base de datos e integración inicial |
| 3 | Panel, detalle, reseñas, categorías, historial y dataset simulado |
| 4 | Reporte Excel, pruebas, manejo de errores, documentación, despliegue y preparación de la demo |

## 9. Supuestos

- El proveedor externo permite el acceso programático a los datos requeridos bajo sus condiciones vigentes.
- Una cuenta, clave o cuota de desarrollo estará disponible para la demostración.
- La cantidad de reseñas devuelta puede ser limitada; el demo trabajará con lo autorizado.
- Los datos operativos serán ficticios y no contendrán información personal.
- Cinco sucursales son suficientes para demostrar comparación y tendencias.
- La aplicación se utilizará con fines de aprendizaje y portafolio, no en producción.

## 10. Restricciones

- Presupuesto mínimo o nulo para servicios externos.
- Desarrollo por una sola persona.
- Alcance limitado para priorizar finalización y calidad.
- No se utilizará scraping que contravenga condiciones de uso.
- No se utilizarán nombres, logotipos ni datos internos de empresas reales.
- Las credenciales y secretos no se almacenarán en el repositorio.

## 11. Dependencias

- Disponibilidad y condiciones del proveedor público.
- Acceso a una base de datos compatible con .NET.
- Entorno de despliegue para ASP.NET Core.
- Librería seleccionada para exportación a Excel.

## 12. Riesgos iniciales

Los riesgos y sus respuestas se mantienen en el [registro de decisiones, supuestos y riesgos](06-decisiones-supuestos-riesgos.md). Los principales son:

- restricciones, costo o cambios del proveedor externo;
- disponibilidad limitada de reseñas;
- crecimiento excesivo del alcance;
- confusión entre datos reales y simulados;
- inversión desproporcionada en diseño visual en lugar de integración y reportes.

## 13. Aprobación de la línea base

| Rol | Nombre | Estado |
|---|---|---|
| Responsable y propietario del demo | Jaime Bolaños | Pendiente de confirmación formal |

Los cambios posteriores que afecten objetivos, alcance, datos o integraciones deberán registrarse antes de implementarse.

