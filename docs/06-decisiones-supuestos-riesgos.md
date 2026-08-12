# Sucursal 360

## Registro de decisiones, supuestos, pendientes y riesgos

| Campo | Valor |
|---|---|
| Versión | 0.1 |
| Fecha | 12 de agosto de 2026 |
| Estado | Activo |
| Responsable | Jaime Bolaños |

## 1. Propósito

Mantener en un único lugar las decisiones que gobiernan el demo, los supuestos utilizados para avanzar, las definiciones pendientes, las funciones diferidas y los riesgos. Este registro deberá actualizarse antes de cambiar el alcance o convertir un supuesto en requisito.

## 2. Estados

| Estado | Significado |
|---|---|
| **Bloqueado** | Decisión confirmada que forma parte de la línea base |
| **Pendiente** | Requiere investigación o decisión antes de una actividad determinada |
| **Diferido** | Se reconoce su valor, pero queda fuera de la primera versión |
| **Supuesto** | Se acepta temporalmente para diseñar o estimar |
| **Riesgo** | Evento incierto que puede afectar alcance, tiempo, costo o calidad |

## 3. Decisiones bloqueadas

| ID | Decisión | Justificación | Impacto |
|---|---|---|---|
| D-01 | El proyecto será una demostración independiente. | Evitar mezclarlo con proyectos reales del responsable. | Repositorio, datos y documentación propios. |
| D-02 | La organización de ejemplo será ficticia. | Evitar atribuir procesos o datos internos a una empresa real. | Se utilizará Café Horizonte y el aviso de independencia. |
| D-03 | La solución se enfocará en gerencia y comparación de sucursales. | Es la propuesta de valor principal. | No se construirá un POS operativo. |
| D-04 | La integración con información pública será central. | Demuestra integración de sistemas de forma visible. | Requiere proveedor autorizado y adaptador. |
| D-05 | La información POS/ERP será simulada. | No existe acceso a sistemas internos reales. | Debe identificarse en todas las vistas y reportes. |
| D-06 | La integración POS/ERP real quedará para el futuro. | Mantener un alcance pequeño y honesto. | Solo se documenta el punto de extensión. |
| D-07 | No se implementarán alertas ni creación de tareas. | Su valor no compensa la complejidad para este demo. | No habrá integración de salida en la primera versión. |
| D-08 | La categorización de reseñas será manual. | Evitar ampliar el alcance hacia IA o NLP. | Se requiere interfaz simple de asignación. |
| D-09 | El reporte principal se exportará a Excel. | Demostrar reportes con una sola salida prioritaria. | PDF queda diferido. |
| D-10 | La implementación usará .NET y tecnologías relacionadas. | Objetivo de actualización y demostración profesional. | Arquitectura y herramientas se definirán en diseño. |
| D-11 | La documentación será en español y Markdown. | Facilitar revisión, versionado y uso por IA. | Los artefactos se mantendrán junto al código. |

## 4. Decisiones pendientes

| ID | Definición pendiente | Necesaria antes de | Criterio para resolver | Responsable |
|---|---|---|---|---|
| P-01 | Proveedor definitivo de datos públicos | Diseño final de integración | Cobertura, términos, costo, campos y reseñas disponibles | Responsable del demo |
| P-02 | Establecimientos públicos que se usarán en la demostración | Datos semilla | Uso legítimo, cobertura suficiente y presentación sin afiliación | Responsable del demo |
| P-03 | Motor de base de datos | Creación de infraestructura | Compatibilidad .NET, costo y facilidad de despliegue | Responsable del demo |
| P-04 | Mecanismo de autenticación | Diseño de seguridad | Simplicidad, roles y despliegue | Responsable del demo |
| P-05 | Librería de exportación Excel | Desarrollo del reporte | Licencia, compatibilidad y mantenibilidad | Responsable del demo |
| P-06 | Umbral de reseña de baja calificación | Configuración funcional | Convención del demo y claridad del KPI | Responsable del demo |
| P-07 | Plataforma de despliegue | Preparación de ambiente | Costo, soporte de .NET y base de datos | Responsable del demo |

## 5. Funciones diferidas

| ID | Función diferida | Razón | Posible fase futura |
|---|---|---|---|
| F-01 | Integración real con POS o ERP | No existe sistema autorizado | Piloto empresarial |
| F-02 | Sincronización automática programada | No es necesaria para demostrar el flujo | Versión 1.1 |
| F-03 | Alertas por correo o mensajería | Amplía soporte y configuración | Versión 1.1 |
| F-04 | Creación y seguimiento de tareas | Requiere un flujo de trabajo adicional | Versión 2 |
| F-05 | Clasificación automática y sentimiento con IA | Aumenta complejidad y riesgo interpretativo | Experimento separado |
| F-06 | Respuesta a reseñas | Requiere autorización y otra integración | Piloto empresarial |
| F-07 | Exportación PDF | Excel cubre el objetivo inicial | Versión 1.1 |
| F-08 | Aplicación móvil | La web adaptable es suficiente | Según necesidad |
| F-09 | Operación POS, pagos e inventario | No pertenece al problema seleccionado | Fuera de la línea de producto |

## 6. Supuestos activos

| ID | Supuesto | Validación prevista | Consecuencia si es falso |
|---|---|---|---|
| A-01 | Existe un proveedor autorizado con información útil para el demo. | Spike técnico. | Usar sandbox oficial o reevaluar fuente. |
| A-02 | La API ofrece calificación y cantidad de reseñas por establecimiento. | Consulta de prueba. | Reducir KPI públicos. |
| A-03 | Al menos una selección de reseñas está disponible legalmente. | Revisar respuesta y términos. | El demo se enfocará en indicadores sin detalle de reseñas. |
| A-04 | Cinco sucursales son suficientes para mostrar comparación. | Revisión de wireframes y demo. | Ajustar datos semilla sin cambiar arquitectura. |
| A-05 | El proyecto puede completarse en cuatro semanas a tiempo parcial. | Seguimiento semanal. | Reducir elementos visuales o categorías, no controles críticos. |
| A-06 | Un dataset mensual simulado es suficiente para la visión POS/ERP. | Revisión del reporte. | Aumentar período o granularidad sin conexión real. |
| A-07 | Un único reporte Excel demuestra la capacidad de reporting. | Validación del guion de demo. | Agregar otra vista, no necesariamente otro formato. |
| A-08 | El usuario final utiliza un navegador moderno de escritorio. | Definición del ambiente. | Ajustar compatibilidad y pruebas. |

## 7. Registro de riesgos

| ID | Riesgo | Prob. | Impacto | Respuesta planificada | Indicador |
|---|---|:---:|:---:|---|---|
| R-01 | La API tiene costo, cuota o exige facturación no disponible. | Media | Alto | Ejecutar spike temprano; limitar llamadas; almacenar resultados permitidos; evaluar sandbox. | No se logra una llamada autorizada. |
| R-02 | La fuente entrega pocas reseñas para un análisis convincente. | Alta | Medio | Diseñar el demo para reconocer cobertura; complementar pruebas con datos mock claramente identificados. | Menos reseñas que las necesarias para filtros. |
| R-03 | Las condiciones impiden almacenar o mostrar ciertos campos. | Media | Alto | Revisar términos; guardar solo lo permitido; separar metadatos de contenido. | Restricción detectada en documentación del proveedor. |
| R-04 | El alcance crece hacia POS, alertas, IA o CRM. | Alta | Alto | Aplicar exclusiones y registrar nuevas ideas como diferidas. | Se agrega trabajo no asociado a criterios de éxito. |
| R-05 | Los evaluadores interpretan datos simulados como reales. | Media | Alto | Etiquetas persistentes, notas metodológicas y aviso en exportación. | Pantalla o reporte sin identificación. |
| R-06 | El proveedor no está disponible durante la demostración. | Media | Medio | Mostrar último resultado exitoso; preparar modo demo con respuesta capturada permitida o mock. | Errores repetidos o latencia excesiva. |
| R-07 | Se exponen claves o detalles técnicos sensibles. | Baja | Alto | Secretos fuera del repositorio; configuración por ambiente; revisión previa a publicación. | Credencial detectada en commit o log. |
| R-08 | El reporte y las gráficas consumen más tiempo que la integración. | Media | Medio | Priorizar tabla y Excel funcional antes de refinamiento visual. | Integración no terminada al final de semana 2. |
| R-09 | La categorización manual se interpreta como análisis automático. | Media | Medio | Mostrar usuario y fecha de asignación; documentar que es manual. | Etiquetas sin procedencia. |
| R-10 | Se usan marcas o establecimientos reales de forma que sugiera afiliación. | Baja | Alto | Aviso visible, nombres ficticios y uso limitado a datos públicos permitidos. | Material de demo sin descargo. |

## 8. Elementos por validar

| ID | Elemento | Método | Resultado esperado |
|---|---|---|---|
| V-01 | Viabilidad del proveedor | Spike de API | Lista real de campos, costos, límites y términos |
| V-02 | Comprensión del panel | Revisión de wireframe | El usuario identifica rápidamente mejor, peor y desactualizada |
| V-03 | Utilidad de categorías | Prueba con reseñas de muestra | Las categorías cubren los temas principales sin solapamiento excesivo |
| V-04 | Claridad de datos simulados | Prueba de aceptación | Ningún evaluador los confunde con datos reales |
| V-05 | Tolerancia a fallos | Prueba de integración | La consulta histórica permanece disponible |
| V-06 | Valor del reporte | Revisión del Excel | Permite una comparación gerencial sin consultar la aplicación |

## 9. Control de cambios

Un cambio requiere actualizar este registro cuando:

- incorpora una función actualmente fuera de alcance;
- cambia el origen o tratamiento de datos;
- modifica un KPI o regla de negocio;
- afecta el plazo de referencia;
- introduce una nueva integración;
- convierte datos simulados en datos reales;
- modifica roles o permisos.

## 10. Historial

| Versión | Fecha | Cambio | Autor |
|---|---|---|---|
| 0.1 | 12/08/2026 | Línea base inicial de decisiones, supuestos y riesgos | Jaime Bolaños |

