# Sucursal 360

## Requisitos de negocio y del sistema

| Campo | Valor |
|---|---|
| Versión | 0.1 |
| Fecha | 12 de agosto de 2026 |
| Estado | Línea base de requisitos |
| Alcance de referencia | [Alcance, objetivos y criterios de éxito](02-alcance-objetivos.md) |

## 1. Propósito

Traducir el problema gerencial en requisitos verificables para la primera versión del demo. Los requisitos se mantienen independientes de un proveedor específico siempre que sea posible.

## 2. Roles y permisos

| Función | Gerente corporativo | Gerente de sucursal | Administrador |
|---|:---:|:---:|:---:|
| Ver panel corporativo | Sí | No | Sí |
| Ver todas las sucursales | Sí | No | Sí |
| Ver sucursal asignada | Sí | Sí | Sí |
| Consultar reseñas y categorías | Sí | Sí, de su sucursal | Sí |
| Asignar categorías | Sí | Sí, de su sucursal | Sí |
| Exportar reporte gerencial | Sí | No | Sí |
| Mantener sucursales | No | No | Sí |
| Ejecutar sincronización | No | No | Sí |
| Consultar bitácora técnica | No | No | Sí |

## 3. Flujo funcional principal

1. El administrador registra o habilita una sucursal.
2. Configura su identificador en el proveedor externo.
3. Ejecuta una sincronización general o individual.
4. El sistema consulta, valida y almacena la información autorizada.
5. El sistema registra el resultado de la operación.
6. El gerente consulta el panel y aplica filtros.
7. Revisa el detalle, historial y reseñas de una sucursal.
8. Asigna categorías cuando sea necesario.
9. Exporta el reporte gerencial.

## 4. Requisitos de negocio

| ID | Requisito | Prioridad |
|---|---|---|
| RN-01 | La gerencia debe poder comparar el desempeño público de las sucursales desde una única aplicación. | Alta |
| RN-02 | La información debe mostrar su fuente y fecha de última actualización. | Alta |
| RN-03 | Debe conservarse información histórica suficiente para observar variaciones. | Alta |
| RN-04 | Las reseñas deben poder organizarse en categorías útiles para la operación. | Alta |
| RN-05 | Los indicadores simulados deben distinguirse claramente de los datos externos. | Alta |
| RN-06 | La gerencia debe poder generar un reporte uniforme para seguimiento. | Alta |
| RN-07 | El fallo de una fuente externa no debe impedir la consulta de la última información almacenada. | Alta |
| RN-08 | La gerencia debe poder cruzar percepcion del cliente, categorias internas y metricas operativas para detectar riesgos y oportunidades por sucursal. | Alta |
| RN-09 | El administrador debe poder diagnosticar el resultado de las sincronizaciones. | Media |

## 5. Requisitos funcionales

### 5.1 Acceso y usuarios

| ID | Requisito funcional | Criterio de aceptación resumido |
|---|---|---|
| RF-01 | El sistema permitirá iniciar y cerrar sesión. | Solo usuarios activos con credenciales válidas ingresan. |
| RF-02 | El sistema aplicará permisos según rol. | Un gerente de sucursal no puede consultar otras sucursales ni funciones administrativas. |
| RF-03 | El administrador podrá asociar un usuario gerente con una sucursal. | La asignación determina el alcance de consulta del usuario. |

### 5.2 Sucursales

| ID | Requisito funcional | Criterio de aceptación resumido |
|---|---|---|
| RF-04 | El administrador podrá crear, editar, activar e inactivar sucursales. | Los campos obligatorios se validan y una sucursal inactiva no aparece en el panel normal. |
| RF-05 | Cada sucursal podrá almacenar un identificador externo y el proveedor correspondiente. | No se permite duplicar la combinación proveedor-identificador. |
| RF-06 | El sistema mostrará dirección, horario y atributos disponibles del establecimiento. | Se muestra el último valor exitosamente sincronizado y su fecha. |

### 5.3 Integración y sincronización

| ID | Requisito funcional | Criterio de aceptación resumido |
|---|---|---|
| RF-07 | El administrador podrá sincronizar una sucursal. | La operación finaliza con estado exitoso, parcial o fallido. |
| RF-08 | El administrador podrá iniciar la sincronización de todas las sucursales activas. | El resultado individual de cada sucursal queda registrado. |
| RF-09 | El sistema validará y transformará la respuesta externa al modelo interno. | Campos inválidos no dañan registros existentes y quedan documentados. |
| RF-10 | Cada ejecución quedará en una bitácora. | Se registra inicio, fin, proveedor, sucursal, estado, cantidad procesada y mensaje. |
| RF-11 | Una falla externa no eliminará la información válida anterior. | El usuario puede consultar el último dato exitoso y ve una advertencia de desactualización. |

### 5.4 Indicadores y panel

| ID | Requisito funcional | Criterio de aceptación resumido |
|---|---|---|
| RF-12 | El panel mostrará todas las sucursales autorizadas para el usuario. | Cada tarjeta muestra calificación, cantidad de reseñas, variación y última actualización. |
| RF-13 | El usuario podrá ordenar y filtrar sucursales. | Se filtra al menos por estado, rango de calificación y fecha de actualización. |
| RF-14 | El sistema mostrará el historial de calificación y cantidad de reseñas. | La gráfica o tabla utiliza instantáneas fechadas de la sucursal. |
| RF-15 | El detalle mostrará datos públicos y operativos simulados en secciones separadas. | La sección simulada utiliza una etiqueta persistente y visible. |
| RF-26 | El panel gerencial comparará sucursales combinando datos de reseñas, categorías manuales y operación simulada. | La tabla principal muestra calificación, reseñas, categoría principal, ventas, transacciones, ticket promedio y nivel de atención. |
| RF-27 | El panel gerencial permitirá filtrar por período, sucursal, categoría, calificación y métrica operativa. | Al aplicar filtros, resumen, comparación, categorías y gráficas se actualizan de forma coherente. |
| RF-28 | El sistema mostrará una matriz de categoría contra desempeño operativo. | Cada categoría muestra cantidad de reseñas, calificación promedio, sucursales afectadas y métricas operativas del período. |
| RF-29 | El sistema generará recomendaciones gerenciales por reglas transparentes. | Las recomendaciones indican la señal usada, no afirman causalidad y conservan las etiquetas de fuente. |

### 5.5 Reseñas y categorías

| ID | Requisito funcional | Criterio de aceptación resumido |
|---|---|---|
| RF-16 | El sistema almacenará las reseñas que el proveedor permita recuperar y conservar. | Se evita duplicación según el identificador externo o una clave técnica definida. |
| RF-17 | El usuario podrá filtrar reseñas por sucursal, período, calificación y categoría. | Los filtros pueden combinarse y restablecerse. |
| RF-18 | Un usuario autorizado podrá asignar una o más categorías a una reseña. | Las asignaciones se guardan sin modificar el texto externo original. |
| RF-19 | El sistema mostrará el conteo de reseñas por categoría. | Los conteos respetan filtros y advierten que una reseña puede aparecer en varias categorías. |

### 5.6 Datos simulados

| ID | Requisito funcional | Criterio de aceptación resumido |
|---|---|---|
| RF-20 | El demo cargará indicadores operativos simulados por sucursal y fecha. | Se dispone de ventas netas, transacciones y ticket promedio para el período de muestra. |
| RF-21 | Los datos simulados se identificarán en pantalla y exportación. | Ningún indicador ficticio aparece sin la etiqueta **Datos simulados**. |
| RF-22 | El sistema validará el formato del dataset simulado. | Filas inválidas se rechazan con un mensaje comprensible y no alteran registros válidos. |

### 5.7 Reporte gerencial

| ID | Requisito funcional | Criterio de aceptación resumido |
|---|---|---|
| RF-23 | El gerente corporativo podrá generar un reporte por período. | El reporte refleja los filtros seleccionados y la fecha de generación. |
| RF-24 | El reporte incluirá comparación de sucursales, tendencias y categorías. | Las definiciones coinciden con el catálogo de KPI. |
| RF-25 | El reporte podrá exportarse a Excel. | El archivo abre correctamente e identifica fuente, corte y datos simulados. |

## 6. Requisitos no funcionales

| ID | Área | Requisito |
|---|---|---|
| RNF-01 | Usabilidad | Un usuario gerencial podrá acceder al panel y comprender los indicadores sin capacitación técnica. |
| RNF-02 | Rendimiento | Las vistas principales responderán en un máximo objetivo de 3 segundos con el volumen del demo, excluyendo sincronizaciones externas. |
| RNF-03 | Resiliencia | La consulta normal funcionará con datos almacenados aunque el proveedor externo esté temporalmente indisponible. |
| RNF-04 | Seguridad | Se aplicará autenticación, autorización por rol, protección de secretos y validación de entradas. |
| RNF-05 | Privacidad | El demo no almacenará datos personales sensibles; los campos de autor se limitarán a lo permitido por la fuente. |
| RNF-06 | Auditabilidad | Las sincronizaciones y cambios de categorización registrarán fecha y usuario cuando corresponda. |
| RNF-07 | Mantenibilidad | La integración externa se implementará detrás de una interfaz para permitir cambiar de proveedor. |
| RNF-08 | Portabilidad | La solución deberá ejecutarse en un entorno compatible con ASP.NET Core y una base de datos relacional soportada. |
| RNF-09 | Calidad de datos | Se aplicarán restricciones de unicidad, tipos apropiados y validaciones antes de persistir información. |
| RNF-10 | Observabilidad | Los errores técnicos tendrán un identificador de correlación y se registrarán sin exponer secretos al usuario. |
| RNF-11 | Accesibilidad | Las vistas principales usarán etiquetas, contraste y navegación básica por teclado. |
| RNF-12 | Trazabilidad | Cada indicador mostrará fuente y fecha de corte. |

## 7. Reglas de negocio

| ID | Regla |
|---|---|
| RB-01 | Una sucursal activa debe tener un nombre, código interno y configuración externa válida para sincronizarse. |
| RB-02 | La combinación de proveedor e identificador externo debe ser única. |
| RB-03 | La calificación mostrada será la recibida del proveedor; Sucursal 360 no la recalculará. |
| RB-04 | Una instantánea no sustituirá a otra; se conservarán registros fechados para análisis histórico. |
| RB-05 | Solo una sincronización de la misma sucursal podrá ejecutarse simultáneamente. |
| RB-06 | Una respuesta parcial no eliminará valores válidos anteriores que el proveedor no devolvió. |
| RB-07 | Una reseña podrá pertenecer a cero, una o varias categorías. |
| RB-08 | El texto original de una reseña no será modificado por la categorización. |
| RB-09 | Los indicadores simulados se almacenarán y mostrarán separadamente de la información pública. |
| RB-10 | El ticket promedio simulado será ventas netas simuladas dividido entre transacciones simuladas; si no existen transacciones se mostrará sin valor. |
| RB-11 | Las variaciones se calcularán solamente entre instantáneas comparables de la misma sucursal y proveedor. |
| RB-12 | Toda exportación indicará fecha de corte, fuente y naturaleza simulada de los indicadores internos. |
| RB-13 | Una categoría no implica sentimiento por sí sola; la calificación de la reseña determina si la señal se interpreta como positiva, neutra o negativa. |
| RB-14 | Las recomendaciones del tablero son reglas de apoyo gerencial y no prueban causalidad entre reseñas y ventas. |

## 8. Historias de usuario prioritarias

### HU-01 - Comparar sucursales

Como gerente corporativo, quiero comparar calificación, cantidad de reseñas y variación de las sucursales para identificar cuáles requieren revisión.

### HU-02 - Comprender una tendencia

Como gerente, quiero consultar el historial de una sucursal para distinguir una observación puntual de una tendencia.

### HU-03 - Analizar temas

Como gerente, quiero filtrar y categorizar reseñas para reconocer temas recurrentes en la experiencia del cliente.

### HU-04 - Preparar seguimiento

Como gerente corporativo, quiero exportar un reporte uniforme para utilizarlo en una reunión de seguimiento.

### HU-05 - Controlar la actualización

Como administrador, quiero conocer el resultado de cada sincronización para resolver problemas de integración.

### HU-06 - Mostrar una integración futura

Como evaluador del demo, quiero ver indicadores operativos simulados claramente identificados para comprender cómo se incorporaría un POS o ERP real.

### HU-07 - Comparar experiencia contra operación

Como gerente corporativo, quiero cruzar categorías de reseñas con ventas, transacciones y ticket promedio para priorizar acciones por sucursal.

### HU-08 - Detectar riesgos y oportunidades

Como gerente corporativo, quiero ver recomendaciones basadas en reglas para identificar sucursales con baja calificación, bajo desempeño, quejas recurrentes o datos faltantes.

## 9. Trazabilidad inicial

| Objetivo | Requisitos relacionados | Evidencia futura |
|---|---|---|
| Comparar sucursales | RN-01, RF-12 a RF-15, RF-26 a RF-29 | Prueba de aceptación del panel analitico |
| Integrar datos públicos | RN-02, RF-05 a RF-11 | Prueba de integración y bitácora |
| Analizar reseñas | RN-04, RF-16 a RF-19 | Prueba de filtros y categorías |
| Mostrar visión futura POS/ERP | RN-05, RF-20 a RF-22 | Verificación de etiquetas y validación |
| Generar reportes | RN-06, RF-23 a RF-25 | Archivo Excel validado |
| Mantener continuidad | RN-07, RNF-03 | Prueba con proveedor no disponible |
| Cruzar experiencia y operación | RN-08, RF-26 a RF-29 | Prueba de filtros, matriz y recomendaciones |

## 10. Exclusiones confirmadas

Los requisitos no incluyen respuestas a reseñas, alertas, tareas, análisis automático de sentimiento, conexión real con POS/ERP, inventario, caja, pagos ni facturación. Cualquier incorporación deberá pasar por control de cambios.
