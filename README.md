# AmirCorp ERP

Sistema de gestión para un grupo de empresas peruanas que importan mercadería desde China: compras nacionales, importaciones, inventario, ventas y, más adelante, facturación electrónica con SUNAT.

Hoy el proyecto es una **API REST** (.NET 10 + PostgreSQL). El frontend irá aparte (Vite).

## Estado actual

| Módulo | Estado |
|---|---|
| Autenticación (JWT) y usuarios con roles | ✅ |
| Empresas (varios RUC en una misma instalación) | ✅ |
| Productos (catálogo compartido entre empresas) | ✅ |
| Clientes y proveedores | ✅ |
| Catálogos SUNAT (monedas, unidades, afectación IGV, tipos de documento) | ✅ |
| Compras nacionales con ingreso de stock por lotes | ✅ |
| Importaciones (DUA y costeo), inventario, ventas, guías de remisión | 🚧 Pendiente |
| Facturación electrónica (UBL 2.1, firma, envío a SUNAT) | 🚧 Al final |

El detalle y el orden de trabajo están en [docs/hoja-de-ruta.md](docs/hoja-de-ruta.md).

## Tecnología

- **.NET 10** / ASP.NET Core Web API (C# 14)
- **PostgreSQL 18** con Entity Framework Core 10 (Npgsql)
- **JWT** para autenticación y **BCrypt** para contraseñas
- Arquitectura por capas con casos de uso: ver [docs/arquitectura.md](docs/arquitectura.md)

```
ERP.Domain          Entidades y reglas de negocio. No depende de nada.
ERP.Application     Casos de uso, DTOs y contratos (interfaces).
ERP.Infrastructure  Servicios técnicos: JWT, hash de contraseñas.
ERP.Persistence     EF Core: DbContext, configuraciones, repositorios, consultas, migraciones.
ERP.Api             Controladores, validación de requests, manejo de errores, arranque.
```

## Puesta en marcha

### 1. Requisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 18](https://www.postgresql.org/download/windows/) (incluye pgAdmin)
- Visual Studio 2026 o cualquier editor con el SDK de .NET

### 2. Configurar los secretos
Los secretos **nunca** van en `appsettings.json`. En desarrollo se guardan con User Secrets, fuera del repositorio. Desde la carpeta raíz:

```bash
dotnet user-secrets set "ConnectionStrings:DB" "Host=localhost;Port=5432;Database=erp;Username=postgres;Password=TU_CONTRASEÑA" --project ERP.Api
```
```bash
dotnet user-secrets set "JwtSettings:Secret" "UNA_CLAVE_ALEATORIA_DE_AL_MENOS_32_CARACTERES" --project ERP.Api
```
```bash
dotnet user-secrets set "SuperAdmin:Name" "Tu Nombre" --project ERP.Api
```
```bash
dotnet user-secrets set "SuperAdmin:Email" "tu@correo.com" --project ERP.Api
```
```bash
dotnet user-secrets set "SuperAdmin:Password" "TuContraseñaSegura" --project ERP.Api
```

En Visual Studio también se editan con **clic derecho en ERP.Api → Administrar secretos de usuario**. Todo el detalle está en [docs/configuracion.md](docs/configuracion.md).

### 3. Crear la base de datos
En la Consola del Administrador de Paquetes, con **ERP.Persistence** como proyecto predeterminado:

```
Update-Database
```

O con la CLI (requiere `dotnet tool install --global dotnet-ef`):

```bash
dotnet ef database update --project ERP.Persistence --startup-project ERP.Api
```

### 4. Arrancar la API
Al iniciar por primera vez se crea el SuperAdmin con los datos de tus secrets (verás `SuperAdmin creado.` en la consola). La API escucha en `http://localhost:5117` y `https://localhost:7234`.

Para probarla:
- **`http://localhost:5117/scalar`**: documentación interactiva. Haz login, copia el token en *Authentication* y prueba cualquier endpoint.
- **`http://localhost:5117/openapi/v1.json`**: el contrato OpenAPI. El frontend genera su cliente con tipos a partir de este archivo.
- [ERP.Api/ERP.Api.http](ERP.Api/ERP.Api.http) en Visual Studio: login y ejemplos que reutilizan el token.

Scalar y OpenAPI solo se publican en el entorno de desarrollo.

## Endpoints

Todos, salvo el login, requieren `Authorization: Bearer <token>`. Salvo `api/me`, requieren rol **SuperAdmin** o **Admin**.

| Recurso | Rutas |
|---|---|
| Autenticación | `POST api/auth/login` |
| Mi perfil | `GET api/me` |
| Usuarios | `api/users` · `GET api/users/roles` · `PATCH {id}/activate`, `/deactivate`, `/role`, `/password` |
| Empresas | `api/companies` · `PATCH {id}/activate`, `/deactivate` |
| Productos | `api/products` · `PATCH {id}/activate`, `/deactivate` |
| Clientes y proveedores | `api/partners` · `GET api/partners/identity-document-types` · `PATCH {id}/activate`, `/deactivate` |
| Compras | `api/purchases` · `PATCH {id}/cancel` |
| Catálogos | `GET api/catalogs/countries`, `/currencies`, `/igv-affectations`, `/tax-document-types`, `/units-of-measure`, `/invoice-price-types` |

Cada recurso tiene `POST` (crear), `GET` (listar), `GET {id}` y `PUT {id}` (actualizar), salvo compras, que no se editan: se anulan.

### Formato de errores
Todos los errores de negocio devuelven el mismo cuerpo:

```json
{ "errors": ["El correo ya se encuentra en uso."] }
```

| Código | Cuándo |
|---|---|
| 400 | Datos inválidos o regla de negocio incumplida |
| 401 / 403 | Sin token o sin permisos |
| 404 | El recurso no existe |
| 409 | Duplicado, o los datos fueron modificados por otro usuario al mismo tiempo |

## Documentación

- [Arquitectura y convenciones](docs/arquitectura.md): cómo está organizado el código y cómo agregar un caso de uso.
- [Configuración y secretos](docs/configuracion.md): desarrollo, producción (Railway) y seed del SuperAdmin.
- [Decisiones técnicas](docs/decisiones.md): qué se decidió y por qué.
- [Hoja de ruta](docs/hoja-de-ruta.md): qué falta y en qué orden.
