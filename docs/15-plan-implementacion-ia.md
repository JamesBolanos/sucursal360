# Sucursal 360

## Plan de implementación para desarrollo asistido por IA

| Campo | Valor |
|---|---|
| Versión | 1.0 |
| Fecha | 12 de agosto de 2026 |
| Estado | Plan ejecutable |
| Duración de referencia | 4 semanas a tiempo parcial |
| Enfoque | Cortes verticales pequeños, verificables y explicables |

## 1. Objetivo

Dar a una IA de programación suficiente contexto para construir el demo sin ampliar alcance, inventar reglas o introducir arquitectura innecesaria. El agente debe producir una aplicación comprensible para Jaime Bolaños y dejar decisiones significativas visibles.

## 2. Orden de autoridad documental

Cuando dos documentos parezcan diferentes, aplicar este orden:

1. [06 — Decisiones, supuestos y riesgos](06-decisiones-supuestos-riesgos.md)
2. [02 — Alcance y objetivos](02-alcance-objetivos.md)
3. [04 — Requisitos](04-requisitos-negocio.md)
4. [05 — Integraciones, KPI y datos](05-integraciones-kpi-datos.md)
5. DOC-07, DOC-10, DOC-11, DOC-12, DOC-13 y DOC-14 — diseño técnico
6. DOC-08 y DOC-09 — comportamiento y experiencia
7. Este plan — secuencia, no redefinición funcional

Si una contradicción cambia comportamiento, datos, seguridad, proveedor o paquete, el agente se detiene y registra una pregunta. No elige silenciosamente.

## 3. Contexto vinculante para la IA

```yaml
project:
  name: Sucursal 360
  fictional_company: Café Horizonte
  purpose: portfolio_demo_for_dotnet_sql_integrations_and_reporting
  language:
    documentation_and_ui: es
    code_identifiers: en
  target_framework: net10.0
  architecture: one_aspnet_core_mvc_app_plus_mstest
  database: sql_server_recommended_pending_final_confirmation
  hosting: azure_app_service_f1_plus_azure_sql_free_optional
scope:
  branches: 5
  required:
    - role_based_login
    - corporate_dashboard
    - branch_detail_and_history
    - manual_review_categories
    - manual_demo_provider_sync
    - simulated_pos_csv_import
    - management_excel_export
    - integration_log
  optional:
    - live_google_places_view
  excluded:
    - real_pos_or_erp
    - pos_screens
    - alerts
    - tasks
    - ai_or_sentiment
    - automatic_sync
    - pdf
    - mobile_app
    - microservices
data_rules:
  google_content_persistence: false
  demo_content_persistence: true
  simulated_label_required: true
  missing_display: No disponible
  persistence_timezone: UTC
  display_timezone: America/Managua
```

## 4. Herramientas y decisiones visibles

| Área | Selección | Estado | Regla de cambio |
|---|---|---|---|
| SDK | .NET 10 LTS | Confirmada | No bajar de versión sin decisión. |
| Web | ASP.NET Core MVC + Razor | Confirmada | No sustituir por SPA/Blazor. |
| ORM | EF Core 10 | Confirmada | No agregar repositorio genérico. |
| Base | SQL Server Developer/Express/Azure SQL | Recomendada | Puede cambiar a SQLite antes de primera migración. |
| Identidad | ASP.NET Core Identity | Confirmada | No agregar Entra/IdentityServer. |
| HTTP | `HttpClientFactory` + resiliencia Microsoft | Confirmada para Google | No SDK Google. |
| Pruebas | MSTest + MVC Testing | Confirmada | No mezclar marcos. |
| Excel | ClosedXML | Recomendada | Verificar licencia; Open XML SDK solo si se rechaza ClosedXML. |
| Gráficas | Chart.js | Recomendada | Una gráfica simple + tabla accesible. |
| Nube | App Service F1 + Azure SQL Free | Opcional recomendada | Local debe funcionar primero. |

Antes de instalar paquetes, el agente presenta la lista y confirma que coincide con esta tabla. No agregar paquetes por preferencia personal.

## 5. Protocolo de trabajo del agente

Para cada paquete de trabajo:

1. Leer los documentos indicados.
2. Declarar archivos que se crearán/modificarán y requisitos cubiertos.
3. Implementar el corte mínimo.
4. Compilar con warnings tratados conscientemente.
5. Ejecutar pruebas relacionadas.
6. Mostrar resultado y limitaciones.
7. Actualizar trazabilidad/checklist.
8. Crear un commit pequeño solo si Jaime lo solicita o el flujo Git está habilitado.

El agente no debe reescribir archivos ajenos, borrar cambios del usuario, introducir datos reales ni afirmar que una integración opcional fue probada si no hubo llamada autorizada.

## 6. Paquetes de trabajo

### WP-00 — Decisiones previas

| Campo | Valor |
|---|---|
| Objetivo | Cerrar herramientas antes de generar la solución. |
| Leer | DOC-06, DOC-07, DOC-12 |
| Tareas | Confirmar SQL Server vs SQLite; aceptar licencia ClosedXML; confirmar si habrá Azure/Google. |
| Salida | Registro de decisiones actualizado. |
| DoD | No hay paquete crítico marcado como pendiente salvo Google/Azure opcionales. |

### WP-01 — Solución ejecutable

| Campo | Valor |
|---|---|
| Requisitos | RNF-07, RNF-08 |
| Tareas | Crear solución, MVC Individual Accounts, MSTest, `global.json`, configuración y README inicial. |
| Evidencia | `dotnet build` y página inicial. |
| DoD | Compila, prueba vacía pasa, secretos no están versionados. |

### WP-02 — Persistencia, Identity y seed

| Campo | Valor |
|---|---|
| Requisitos | RF-01 a RF-05 |
| Leer | DOC-10, DOC-11, DOC-14 |
| Tareas | Entidades, DbContext, configuraciones, migración, roles, categorías, cinco sucursales y usuarios de desarrollo. |
| Pruebas | Restricciones, roles y asignación obligatoria. |
| DoD | Base se crea desde cero y seed es repetible. |

### WP-03 — Autorización y mantenimiento

| Campo | Valor |
|---|---|
| Requisitos | RF-02 a RF-05 |
| Tareas | `BranchAccessService`, políticas, CRUD sin delete y usuarios/alcance. |
| Pruebas | SEC-T01 a SEC-T05. |
| DoD | Gerente no cruza sucursal ni accede a admin. |

### WP-04 — Proveedor DEMO y sincronización

| Campo | Valor |
|---|---|
| Requisitos | RF-06 a RF-11 |
| Leer | DOC-07, DOC-08, DOC-13 |
| Tareas | DTO, interfaz, fixtures, proveedor, validación, servicio, bitácora y UI admin. |
| Pruebas | CT-01 a CT-03, concurrencia y conservación. |
| DoD | Cinco sucursales sincronizan sin red; error conserva datos previos. |

### WP-05 — Panel y detalle

| Campo | Valor |
|---|---|
| Requisitos | RF-12 a RF-15 |
| Leer | DOC-09, consultas Q-01/Q-02/Q-05 |
| Tareas | Proyecciones, filtros, tabla, una gráfica + tabla histórica y etiquetas. |
| Pruebas | Filtros, nulos, alcance y tiempo objetivo con dataset demo. |
| DoD | Comparación explica fuente, fecha y dato simulado. |

### WP-06 — Reseñas y categorías

| Campo | Valor |
|---|---|
| Requisitos | RF-16 a RF-19 |
| Tareas | Filtros, paginación, conteos, reemplazo atómico, auditoría. |
| Pruebas | Múltiples categorías, quitar categoría, alcance y texto inmutable. |
| DoD | Clasificación se identifica como manual. |

### WP-07 — Importación POS/ERP simulada

| Campo | Valor |
|---|---|
| Requisitos | RF-20 a RF-22 |
| Tareas | Parser, preview, validaciones, confirmación, importación atómica. |
| Pruebas | CT-07, CT-08 y códigos CSV. |
| DoD | Archivo inválido guarda cero; todos los resultados dicen `Datos simulados`. |

### WP-08 — Reporte Excel

| Campo | Valor |
|---|---|
| Requisitos | RF-23 a RF-25 |
| Tareas | Filtros, query model, exportador ClosedXML, cinco hojas, estilos mínimos. |
| Pruebas | CT-09, CT-10; abrir libro con lector automatizado. |
| DoD | Archivo coherente con pantalla y sin fórmulas inyectables. |

### WP-09 — Integración Google opcional

| Campo | Valor |
|---|---|
| Condición | Solo tras `SPIKE-INT-01`, cuenta autorizada y páginas legales. |
| Tareas | Cliente tipado, DTO Google, mapper, atribución y vista efímera. |
| Pruebas | Respuestas grabadas/sanitizadas; red nunca en CI. |
| DoD | Una llamada manual demostrable; cero contenido Google persistido. |

Si no se cumplen condiciones, marcar WP-09 `Omitido justificadamente`; no bloquea el demo.

### WP-10 — Calidad, README y demo

| Campo | Valor |
|---|---|
| Tareas | Pruebas completas, accesibilidad básica, logs, datos seed, guion, capturas y limitaciones. |
| DoD | Build/pruebas limpios, instalación reproducible y recorrido de 5–7 minutos. |

### WP-11 — Azure opcional

| Campo | Valor |
|---|---|
| Condición | Cuenta Azure disponible. |
| Tareas | App Service F1, Azure SQL Free, settings, migración y publicación. |
| Controles | Pausa al límite, presupuesto/alerta, HTTPS, sin clave en repo. |
| DoD | URL carga, login funciona y costo estimado muestra cero. |

## 7. Orden por semanas

| Semana | Paquetes | Resultado demostrable |
|---|---|---|
| 1 | WP-00 a WP-03 | Login, roles, sucursales y base. |
| 2 | WP-04 y WP-05 | Integración demo, bitácora, panel e histórico. |
| 3 | WP-06 a WP-08 | Categorías, CSV simulado y Excel. |
| 4 | WP-09 opcional, WP-10 y WP-11 opcional | Integración en vivo/publicación y demo pulido. |

Si hay retraso, omitir WP-09 y WP-11 primero. No recortar autorización, validaciones, etiquetas ni conservación de datos.

## 8. Matriz mínima de pruebas

| Nivel | Objetivo | Cantidad orientativa |
|---|---|---:|
| Unitarias | Mapeos, fórmulas, validación CSV, categorización | 15–25 |
| Integración EF | Restricciones, consultas y transacciones | 8–12 |
| Integración MVC | Auth, permisos, POST, respuestas | 8–12 |
| Contrato | Fixtures y JSON Google sanitizado | 5–8 |
| Aceptación manual | Recorrido HU-01 a HU-06 | 6 |

La cantidad no es una meta contractual; cubrir riesgos e invariantes es más importante.

## 9. Definition of Done del producto

- [ ] Compila con SDK .NET 10 y se ejecuta localmente siguiendo README.
- [ ] Primera migración crea base y seed ficticio reproducible.
- [ ] Los tres roles y alcance de sucursal están probados.
- [ ] Cinco sucursales sincronizan con fixtures sin internet.
- [ ] Exitoso, Parcial y Fallido son visibles en bitácora.
- [ ] Una falla conserva el último dato válido.
- [ ] Panel, detalle, histórico y reseñas cumplen UX-02 a UX-04.
- [ ] La clasificación es manual y auditable.
- [ ] CSV inválido no persiste parcialmente.
- [ ] Excel contiene las cinco hojas y etiquetas.
- [ ] Ningún dato simulado parece real.
- [ ] Ningún secreto está en Git, logs o documentación.
- [ ] Google y Azure opcionales están implementados o marcados como omitidos.
- [ ] Guion de 5–7 minutos demuestra requisitos del puesto.

## 10. Guion de demostración

1. Explicar el problema gerencial ficticio y el alcance.
2. Entrar como Administrador y ejecutar sincronización de las cinco sucursales.
3. Mostrar una ejecución exitosa, una parcial y una fallida con conservación.
4. Entrar como GerenteCorporativo y comparar el panel.
5. Abrir una sucursal, histórico y datos operativos claramente simulados.
6. Filtrar reseñas y asignar una categoría manual.
7. Exportar y abrir el Excel.
8. Mostrar brevemente interfaces, DTO canónico y pruebas.
9. Opcional: consulta Google en vivo o URL Azure.
10. Cerrar con límites: no es POS, no usa datos internos ni IA.

## 11. Condiciones de parada para la IA

El agente debe preguntar antes de continuar si:

- se necesita cambiar SQL Server por otro motor después de migraciones;
- una librería exige licencia incompatible;
- una función requiere persistir contenido Google;
- un requisito implica POS/ERP real, alertas, tareas o IA;
- una prueba necesita credencial/red real;
- se propone otro proyecto, servicio o paquete no contemplado;
- una decisión contradice DOC-06;
- encuentra cambios del usuario que sería necesario sobrescribir.

## 12. Formato de entrega de cada corte

```text
Resultado:
- requisito(s) cubierto(s)
- comportamiento observable

Archivos:
- creados
- modificados

Verificación:
- comandos ejecutados
- pruebas aprobadas/fallidas

Decisiones o pendientes:
- solo los que requieren intervención

Siguiente corte recomendado:
- un único objetivo pequeño
```

## 13. Referencias internas

- [Investigación técnica](07-investigacion-tecnica-integracion.md)
- [Procesos y casos de uso](08-procesos-casos-uso.md)
- [Diseño UX](09-diseno-experiencia-wireframes.md)
- [Modelo de dominio](10-modelo-dominio-diccionario.md)
- [Modelo de datos](11-modelo-datos-dbml.md)
- [Arquitectura](12-arquitectura-solucion.md)
- [Integraciones](13-diseno-integraciones-contratos.md)
- [Seguridad](14-diseno-seguridad-acceso.md)
