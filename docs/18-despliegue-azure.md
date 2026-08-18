# Sucursal 360

## Despliegue en Microsoft Azure App Service

Objetivo: publicar el demo en Azure con la menor friccion posible, manteniendo datos ficticios y sin guardar secretos en el repositorio.

## 1. Decision de despliegue

Para el primer demo publico se usara:

- Azure App Service.
- Runtime administrado de .NET 10.
- GitHub Actions para despliegue continuo.
- SQLite en archivo persistente bajo `/home/site/data/app.db`.

Esta configuracion es valida para un demo de una sola instancia. No escalar horizontalmente mientras se use SQLite.

## 2. Limitaciones aceptadas

- SQLite no es la base recomendada para produccion real.
- La aplicacion debe ejecutarse en una sola instancia.
- Si el demo crece o se vuelve multiusuario real, migrar a Azure SQL.
- Los datos siguen siendo ficticios.
- No se agregan API live ni datos reales en este despliegue.

## 3. Crear App Service

Desde Azure Portal:

1. Crear un Resource Group, por ejemplo `rg-sucursal360-demo`.
2. Crear un recurso `Web App`.
3. Runtime stack: `.NET 10`.
4. Sistema operativo: Linux.
5. Plan: Basic B1 o superior recomendado para evitar suspensiones frecuentes.
6. Publicar: Code.
7. Crear el recurso.

## 4. Configuracion de la aplicacion

En el App Service, abrir `Settings` > `Environment variables` > `App settings`.

Agregar:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Data Source=/home/site/data/app.db;Cache=Shared
Database__MigrateOnStartup=true
SeedUsers__Enabled=true
SeedUsers__DefaultPassword=<password-demo-seguro>
```

La contrasena debe cumplir:

- minimo 10 caracteres;
- mayuscula;
- minuscula;
- digito.

Despues del primer despliegue exitoso, se puede cambiar:

```text
SeedUsers__Enabled=false
```

Esto evita que el demo vuelva a actualizar usuarios en cada arranque. Si se deja activo, el seeder no imprime contrasenas ni borra usuarios, pero seguira reactivando los usuarios demo.

## 5. Configurar GitHub Actions

El repositorio incluye:

```text
.github/workflows/azure-app-service.yml
```

En GitHub:

1. Abrir el repositorio.
2. Ir a `Settings` > `Secrets and variables` > `Actions`.
3. Crear una variable:

```text
AZURE_WEBAPP_NAME=<nombre-del-app-service>
```

4. Crear un secret:

```text
AZURE_WEBAPP_PUBLISH_PROFILE=<contenido-del-publish-profile>
```

El publish profile se descarga desde el App Service en Azure Portal usando `Get publish profile`.

## 6. Publicar

Con los settings configurados:

1. Ejecutar manualmente el workflow `Deploy Azure App Service`.
2. Esperar que GitHub Actions ejecute restore, build, test, publish y deploy.
3. Abrir la URL del App Service.
4. Iniciar sesion con:

```text
admin@sucursal360.local
corporativo@sucursal360.local
sucursal@sucursal360.local
```

Usar la contrasena configurada en `SeedUsers__DefaultPassword`.

## 7. Verificacion post-despliegue

1. Abrir `/`.
2. Iniciar sesion como administrador.
3. Abrir `Integraciones`.
4. Ejecutar `Sincronizar todas`.
5. Abrir `Datos simulados`.
6. Importar `samples/simulated-operational-metrics.csv`.
7. Abrir `Panel`.
8. Confirmar que se muestran ventas, ticket y resumen ejecutivo.
9. Abrir `Reportes` y exportar Excel.

## 8. Cuando migrar a Azure SQL

Migrar si:

- se necesita mas de una instancia;
- se espera concurrencia real;
- se necesita respaldo formal;
- se usaran datos reales;
- el demo pasa a piloto.

En ese caso cambiar `UseSqlite` por un proveedor compatible, agregar el paquete EF correspondiente y crear una migracion controlada.
