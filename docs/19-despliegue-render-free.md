# Sucursal 360

## Despliegue en Render Free

Objetivo: publicar el demo sin Azure y sin base de datos externa, aceptando que Render Free usa filesystem efimero.

## 1. Decision de despliegue

Para la version demo gratuita se usara:

- Render Free Web Service.
- Docker.
- SQLite en `/tmp/sucursal360/app.db`.
- Bootstrap automatico de datos demo en cada arranque.
- Deploy automatico desde GitHub.

Esta ruta busca baja friccion. No es una arquitectura de produccion.

## 2. Que se reconstruye automaticamente

Con `DemoBootstrap__Enabled=true`, el arranque del demo puede crear:

- roles de aplicacion;
- usuarios demo;
- sucursales y categorias base desde migraciones/seed EF;
- snapshots y resenas desde fixtures locales;
- ventas, transacciones y ticket desde `samples/simulated-operational-metrics.csv`;
- categorias iniciales para resenas demo.

## 3. Limitacion aceptada

Render Free puede reiniciar o dormir el servicio. El filesystem local se considera efimero.

Por eso el demo usa:

```text
DemoBootstrap__ResetDatabase=true
```

Esto significa:

- el demo vuelve a un estado conocido en cada arranque;
- los cambios manuales no son permanentes;
- no se necesita Postgres para esta etapa;
- no se deben usar datos reales.

## 4. Archivos agregados

```text
Dockerfile
.dockerignore
render.yaml
```

`render.yaml` define un Web Service gratuito con runtime Docker.

## 5. Crear el servicio en Render

Desde Render Dashboard:

1. Click en `New`.
2. Seleccionar `Blueprint`.
3. Conectar el repositorio `sucursal360`.
4. Render detectara `render.yaml`.
5. Revisar el servicio `sucursal360`.
6. Configurar el valor secreto solicitado:

```text
SeedUsers__DefaultPassword=<password-demo-seguro>
```

La contrasena debe cumplir:

- minimo 10 caracteres;
- mayuscula;
- minuscula;
- digito.

Ejemplo:

```text
DemoPassword123
```

No usar una contrasena personal.

## 6. Variables configuradas por render.yaml

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Data Source=/tmp/sucursal360/app.db;Cache=Shared
Database__MigrateOnStartup=true
SeedUsers__Enabled=true
DemoBootstrap__Enabled=true
DemoBootstrap__ResetDatabase=true
DemoBootstrap__OperationalMetricsCsvPath=/app/samples/simulated-operational-metrics.csv
```

La unica variable que debe completarse manualmente como secreto es:

```text
SeedUsers__DefaultPassword
```

## 7. Verificacion

Despues del deploy:

1. Abrir la URL publica de Render.
2. Iniciar sesion con:

```text
admin@sucursal360.local
corporativo@sucursal360.local
sucursal@sucursal360.local
```

3. Usar la contrasena configurada en Render.
4. Abrir `Panel`.
5. Confirmar que se muestran ventas y ticket.
6. Abrir `Resenas` y confirmar que hay resenas categorizadas.
7. Abrir `Reportes` y exportar Excel.

## 8. Cuando cambiar de estrategia

Migrar a Postgres/Neon, Supabase o una base administrada si:

- se necesita persistir cambios manuales;
- se quiere conservar datos entre reinicios;
- se conecta un proveedor live;
- el demo pasa a piloto;
- se usaran datos reales.

Para ese paso futuro, mantener SQLite local y usar Postgres en hosting requerira agregar `Npgsql.EntityFrameworkCore.PostgreSQL` y probar el modelo EF con ambos proveedores.
