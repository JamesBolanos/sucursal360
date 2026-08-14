# Sucursal 360

## Guion demo 5-7 minutos

Objetivo: demostrar una app gerencial que integra datos publicos/demo, resenas sociales, datos operativos simulados, categorizacion interna y reportes ejecutivos para una cadena ficticia de cafeterias.

## Preparacion

Antes de presentar:

1. Ejecutar la aplicacion en ambiente `Development`.
2. Tener configurado `SeedUsers:DefaultPassword`.
3. Iniciar sesion con `admin@sucursal360.local`.
4. Tener listo el archivo `samples/simulated-operational-metrics.csv`.

## Recorrido

### 1. Contexto

Tiempo: 30 segundos.

Mensaje:

Sucursal 360 consolida informacion publica de reputacion, resenas, datos operativos simulados y categorizacion interna para comparar sucursales de Cafe Horizonte. El demo usa datos ficticios y controlados para que funcione sin depender de nube ni APIs externas.

Pantalla:

- `Inicio` o `Panel`.

### 2. Integracion publica demo

Tiempo: 60 segundos.

Accion:

1. Abrir `Integraciones`.
2. Mostrar las sucursales sincronizables.
3. Click en `Sincronizar todas`.

Mensaje:

Este paso simula consumir un proveedor externo. La app normaliza los datos, guarda snapshots, reseñas y bitacora de ejecucion. Si hay error, conserva datos previos y registra correlacion.

### 3. Panel gerencial analitico

Tiempo: 90-120 segundos.

Accion:

1. Abrir `Panel`.
2. Mostrar controles de periodo, sucursal, categoria, calificacion y metrica.
3. Comparar sucursales por reputacion, categoria principal, ventas, transacciones y ticket.
4. Mostrar recomendaciones y explicar que son reglas transparentes.

Mensaje:

Este es el centro de decision. El gerente puede comparar percepcion del cliente contra metricas operativas simuladas. Las categorias vienen del analisis interno; las estrellas indican si el tema es positivo o negativo. La app no afirma causalidad ni inventa valores: cuando falta informacion muestra `No disponible`.

### 4. Detalle de sucursal

Tiempo: 60 segundos.

Accion:

1. Click en `Ver` sobre una sucursal.
2. Mostrar ultimo snapshot, ultima sincronizacion, tendencia e historico.

Mensaje:

El detalle permite explicar si una observacion es puntual o parte de una tendencia. La correlacion de sincronizacion sirve para diagnosticar problemas sin exponer detalles tecnicos al usuario.

### 5. Resenas y categorias

Tiempo: 90 segundos.

Accion:

1. Abrir `Resenas`.
2. Filtrar por sucursal o calificacion.
3. Mostrar estrellas.
4. Asignar categorias a una resena.

Mensaje:

Las categorias son temas gerenciales: servicio, precio, calidad, limpieza. La calificacion en estrellas da la senal positiva o negativa. Por ejemplo, `Servicio` con 2 estrellas indica problema de servicio; `Servicio` con 5 estrellas indica fortaleza de servicio. Los cambios son manuales y auditados.

### 6. Datos simulados POS/ERP

Tiempo: 60 segundos.

Accion:

1. Abrir `Datos simulados`.
2. Cargar `samples/simulated-operational-metrics.csv`.
3. Click en `Validar CSV`.
4. Click en `Confirmar importacion`.
5. Volver al `Panel`.

Mensaje:

Este paso representa una futura integracion POS/ERP sin conectarse a un sistema real. El importador valida todo el archivo antes de guardar; si una fila falla, no persiste nada parcial. Al volver al panel, esas metricas permiten comparar ventas, transacciones y ticket contra la senal de cliente.

### 7. Reporte Excel

Tiempo: 60 segundos.

Accion:

1. Abrir `Reportes`.
2. Exportar Excel.
3. Mostrar hojas principales si se abre el archivo.

Mensaje:

El cierre del demo es un entregable gerencial: resumen, sucursales, tendencias, categorias y operacion simulada. El reporte identifica fuentes, periodo y datos simulados.

## Cierre

Mensaje:

El valor del demo no esta en un algoritmo complejo, sino en integrar datos de varias fuentes, conservar historia, separar origenes, permitir analisis manual y convertir todo eso en una vista gerencial accionable. La siguiente fase podria ser una API live opcional o despliegue publico.

## Preguntas esperadas

### Esto usa datos reales?

No en el recorrido principal. Usa datos ficticios y fixtures locales para asegurar que el demo sea reproducible.

### Las categorias detectan sentimiento automaticamente?

No. Las categorias son manuales y representan temas. La calificacion en estrellas indica la senal positiva o negativa.

### Puede integrarse con Google Places?

Si, como proveedor live opcional, pero el demo base no depende de eso. Cualquier uso live debe respetar terminos, costos, billing y reglas de almacenamiento.

### Puede conectarse a un POS real?

No en esta version. El CSV simulado demuestra el punto de extension sin depender de un sistema externo real.
