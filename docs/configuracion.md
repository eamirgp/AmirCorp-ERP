# Configuración y secretos

## Qué va en cada lugar

| Dónde | Qué contiene | ¿Va a GitHub? |
|---|---|---|
| `ERP.Api/appsettings.json` | Configuración no secreta: logging, `JwtSettings:Issuer`, `Audience`, `ExpirationInMinutes` | Sí |
| **User Secrets** (tu PC) | Secretos de desarrollo | No |
| **Variables de entorno** (servidor) | Secretos de producción | No |

.NET combina las fuentes en este orden, y cada una pisa a la anterior:

```
appsettings.json → appsettings.{Entorno}.json → User Secrets (solo en Development) → variables de entorno
```

El código siempre lee `configuration["Clave"]` y no sabe de dónde viene el valor.

## Claves requeridas

| Clave | Para qué | Si falta |
|---|---|---|
| `ConnectionStrings:DB` | Conexión a PostgreSQL | La API no arranca y muestra qué falta |
| `JwtSettings:Secret` | Firma de los tokens (mínimo 32 caracteres) | La API no arranca y muestra qué falta |
| `SuperAdmin:Name`, `:Email`, `:Password` | Crear el primer usuario | La API arranca con un aviso; solo hacen falta con la base vacía |

Formato de la conexión:

```
Host=localhost;Port=5432;Database=erp;Username=postgres;Password=...
```

## Desarrollo: User Secrets

El proyecto `ERP.Api` tiene un `UserSecretsId` en su `.csproj`. Los valores se guardan en:

```
%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json
```

Ese archivo está **fuera** del repositorio. No está cifrado: la protección consiste en que nunca se sube.

Formas de editarlo:
- Visual Studio: **clic derecho en ERP.Api → Administrar secretos de usuario**.
- Consola: `dotnet user-secrets set "Clave" "Valor" --project ERP.Api` y `dotnet user-secrets list --project ERP.Api`.

Ejemplo de `secrets.json`:

```json
{
  "ConnectionStrings": { "DB": "Host=localhost;Port=5432;Database=erp;Username=postgres;Password=..." },
  "JwtSettings": { "Secret": "..." },
  "SuperAdmin": { "Name": "...", "Email": "...", "Password": "..." }
}
```

Para generar una clave JWT aleatoria (PowerShell):

```powershell
$b = New-Object byte[] 48; [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b); dotnet user-secrets set "JwtSettings:Secret" ([Convert]::ToBase64String($b)) --project ERP.Api
```

## Producción (Railway)

En el servicio de la API, pestaña **Variables**. El `:` se escribe como doble guion bajo:

```
ConnectionStrings__DB
JwtSettings__Secret
SuperAdmin__Name
SuperAdmin__Email
SuperAdmin__Password
```

- Usa valores **distintos** a los de desarrollo, sobre todo la clave JWT.
- Marca como *sealed* las variables secretas.
- Cualquier valor de `appsettings.json` se puede cambiar igual, por ejemplo `JwtSettings__ExpirationInMinutes=30`.
- Pendiente al desplegar: que la API escuche en el puerto que asigna Railway y ajustar el manejo de HTTPS detrás de su proxy.

## SuperAdmin inicial

Al arrancar, `SeedSuperAdminAsync()` (en `ERP.Api/Extensions`) ejecuta `EnsureSuperAdminUseCase`:

1. Si ya existe un SuperAdmin, no hace nada.
2. Si no existe, lo crea con la sección `SuperAdmin` de la configuración y la contraseña hasheada con BCrypt.
3. Si faltan datos o son inválidos, muestra un aviso en la consola y la API arranca igual.

Lo registra el "usuario sistema" (`AuditableEntity.SystemUserId`, `Guid.Empty`), porque todavía no hay nadie autenticado.

**En producción:** después del primer arranque, borra `SuperAdmin__Password` de las variables y cambia la contraseña desde la aplicación.

## Migraciones

Las migraciones viven en `ERP.Persistence/Migrations`.

Visual Studio, Consola del Administrador de Paquetes (proyecto predeterminado **ERP.Persistence**):

```
Add-Migration NombreDescriptivo
Update-Database
```

CLI:

```bash
dotnet ef migrations add NombreDescriptivo --project ERP.Persistence --startup-project ERP.Api
```
```bash
dotnet ef database update --project ERP.Persistence --startup-project ERP.Api
```

Usa nombres que digan qué cambia (`AddSales`, `AddWarehouseToStockEntry`), y revisa la migración generada antes de aplicarla. La API debe estar detenida para que la compilación no falle por archivos bloqueados.
