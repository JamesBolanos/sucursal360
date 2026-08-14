# Sucursal 360

## Ficha de iniciación del proyecto demo

| Campo | Valor |
|---|---|
| Proyecto | Sucursal 360 - Monitor de desempeño de sucursales |
| Organización de ejemplo | Café Horizonte (empresa ficticia) |
| Tipo de iniciativa | Demostración funcional y portafolio profesional |
| Responsable | Jaime Bolaños |
| Versión | 0.1 |
| Fecha | 12 de agosto de 2026 |
| Estado | Línea base inicial |

> **Declaración de independencia:** Café Horizonte y Sucursal 360 son nombres ficticios utilizados con fines demostrativos. El proyecto no representa, no utiliza información interna y no implica afiliación con Casa del Café, Café Soluble, S.A. ni con otra empresa real. Cuando se utilicen datos públicos de establecimientos, estos se identificarán como información de terceros obtenida mediante un proveedor autorizado.

## 1. Resumen ejecutivo

Sucursal 360 será una aplicación web orientada a la gerencia de una cadena de cafeterías. Su finalidad será consolidar información pública de las sucursales —como calificación, cantidad de reseñas, horarios, dirección y comentarios disponibles— para facilitar la comparación entre locales y la identificación de tendencias que requieren atención.

La primera versión funcionará como una demostración de análisis de negocio, desarrollo en .NET, integración de sistemas, consultas de datos y elaboración de reportes. Los indicadores de ventas, transacciones y ticket promedio serán datos simulados y estarán identificados como tales. Su presencia permitirá representar cómo podría enriquecerse el análisis cuando, en una implementación real, existiera una conexión con el POS o ERP de la organización.

## 2. Oportunidad de negocio

La información sobre la percepción de los clientes suele encontrarse distribuida entre fichas públicas de distintas sucursales. Revisarla manualmente dificulta responder preguntas gerenciales como:

- ¿Qué sucursales tienen mejor o peor calificación?
- ¿En cuáles está disminuyendo la valoración del cliente?
- ¿Qué temas se repiten en los comentarios negativos?
- ¿Existen problemas relacionados con servicio, limpieza, espera, calidad o precio?
- ¿Cómo podría relacionarse esa percepción con indicadores internos en el futuro?

Sucursal 360 centralizará esa información en una vista comparable y conservará instantáneas históricas para mostrar cambios a través del tiempo.

## 3. Propósito del demo

Demostrar la capacidad de transformar una necesidad gerencial en una solución de software pequeña pero coherente, cubriendo:

1. análisis del problema y definición de requerimientos;
2. diseño de una solución orientada a usuarios de negocio;
3. consumo de información desde un servicio externo;
4. almacenamiento y consulta mediante una base de datos relacional;
5. creación de indicadores y reportes gerenciales;
6. validación, manejo de errores y trazabilidad de las sincronizaciones.

## 4. Objetivo de alto nivel

Construir una aplicación web en .NET que permita a un usuario gerencial consultar, comparar y analizar el desempeño público de varias sucursales, complementado con indicadores operativos simulados claramente diferenciados de los datos externos.

## 5. Beneficios esperados

- Vista consolidada de todas las sucursales.
- Identificación rápida de locales con deterioro en sus calificaciones.
- Organización de reseñas por temas relevantes para la operación.
- Historial de calificaciones y cantidad de reseñas por sucursal.
- Reporte gerencial reutilizable para reuniones de seguimiento.
- Evidencia práctica de integración, reportes y desarrollo con tecnologías Microsoft.

## 6. Interesados y usuarios

| Interesado o usuario | Interés principal | Participación en el demo |
|---|---|---|
| Responsable del proyecto | Construir y documentar una demostración profesional | Define, desarrolla, prueba y presenta |
| Gerencia corporativa ficticia | Comparar sucursales y detectar tendencias | Usuario principal representado |
| Gerente de sucursal ficticio | Comprender la situación de su local | Usuario de consulta limitada |
| Administrador del sistema | Mantener sucursales, fuentes y sincronizaciones | Usuario técnico representado |
| Proveedor externo de datos | Entregar información pública autorizada | Sistema externo |
| POS/ERP futuro | Proveer ventas y transacciones reales | Integración diferida; se simula en el demo |

## 7. Productos principales

- Aplicación web demostrativa Sucursal 360.
- Integración con un proveedor de información pública, sujeto a viabilidad técnica y condiciones de uso.
- Base de datos con sucursales, instantáneas de indicadores, reseñas, categorías y registros de sincronización.
- Panel gerencial analitico y detalle por sucursal.
- Reporte gerencial exportable a Excel.
- Dataset operativo simulado.
- Documentación funcional, técnica y de pruebas de las siguientes fases.

## 8. Enfoque de trabajo

El proyecto seguirá el playbook híbrido definido para iniciativas pequeñas:

- prácticas de iniciación y control inspiradas en PMBOK;
- análisis de requerimientos y trazabilidad inspirados en BABOK;
- construcción iterativa mediante entregas pequeñas;
- documentación Markdown como fuente principal para el desarrollador y el agente de IA;
- decisiones no confirmadas tratadas como configurables, diferidas o pendientes.

## 9. Criterio de autorización para avanzar

El proyecto podrá pasar a diseño de solución cuando estén definidos:

- alcance y exclusiones;
- requisitos funcionales y no funcionales prioritarios;
- proveedor externo candidato o estrategia de simulación controlada;
- entidades principales y reglas de sincronización;
- indicadores y reporte de la primera versión;
- decisiones pendientes que no bloquean el desarrollo.

## 10. Referencias internas

- [Alcance, objetivos y criterios de éxito](02-alcance-objetivos.md)
- [Problema y contexto de negocio](03-problema-contexto-negocio.md)
- [Requisitos de negocio y del sistema](04-requisitos-negocio.md)
- [Integraciones, datos, KPI y reportes](05-integraciones-kpi-datos.md)
- [Decisiones, supuestos y riesgos](06-decisiones-supuestos-riesgos.md)
