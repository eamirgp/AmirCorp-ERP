# Arquitectura y convenciones

## Capas y dependencias

```
ERP.Api ─────────┬──> ERP.Application ──> ERP.Domain
                 ├──> ERP.Infrastructure ──> ERP.Application
                 └──> ERP.Persistence ─────> ERP.Application
```

- **Domain** no conoce a nadie. Contiene las entidades, sus reglas y los catálogos (enums con códigos SUNAT).
- **Application** define **qué** hace el sistema (casos de uso) y **qué necesita** de afuera (interfaces en `Contracts/`). No sabe que existe EF Core ni ASP.NET.
- **Persistence** e **Infrastructure** implementan esas interfaces.
- **Api** recibe HTTP, valida el formato del request, llama al caso de uso y convierte el resultado en una respuesta.

## Recorrido de un request

Ejemplo: `POST api/products`.

1. **Controlador** (`ProductController`) recibe `CreateProductRequest`.
2. **`request.Validate()`** revisa formato: requeridos, largos, valores de enum. Si falla → 400.
3. **`request.ToDto()`** lo convierte al DTO de Application.
4. **Caso de uso** (`CreateProductUseCase`) aplica las reglas que necesitan datos: ¿el código ya existe? Si falla devuelve `Result.Failure(errores, ErrorType.Conflict)`.
5. **Dominio** (`Product.Create`) valida sus invariantes y **normaliza** (el código queda en mayúsculas). Si algo es inválido lanza `DomainException` → 400.
6. **Repositorio** + **`IUnitOfWork.SaveChangesAsync()`** guardan. Un solo `SaveChanges` por caso de uso, así que todo se guarda en una sola transacción.
7. **`AuditInterceptor`** completa `CreatedAt/CreatedBy` o `UpdatedAt/UpdatedBy`.
8. El controlador convierte el `Result` en HTTP con `ToActionResult()`.

## Dónde va cada validación

| Nivel | Qué valida | Ejemplo |
|---|---|---|
| Api (`Request.Validate()`) | Formato del request HTTP | "La contraseña debe tener al menos 8 caracteres" |
| Application (caso de uso) | Reglas que necesitan consultar la base | "El correo ya se encuentra en uso" |
| Domain (`Entidad.Create`) | Invariantes de la entidad, siempre | "El RUC debe tener 11 dígitos" |

Las constantes compartidas (largos máximos, `User.PasswordMinLength`) viven en el dominio y se reutilizan en las demás capas.

## Lectura y escritura separadas

- **Escritura** → `Contracts/Persistence/Commands`: repositorios que devuelven entidades con seguimiento de cambios, más `IUnitOfWork`.
- **Lectura** → `Contracts/Persistence/Queries`: consultas `AsNoTracking` que proyectan directo a DTOs. Los listados devuelven `PagedResult<T>`.

## Manejo de errores

| Origen | Manejo | HTTP |
|---|---|---|
| `Result.Failure(…, ErrorType.X)` | `ResultExtensions.ToActionResult()` | 400, 401, 403, 404 o 409 |
| `DomainException` | `DomainExceptionHandler` | 400 |
| `ConcurrencyException` | `ConcurrencyExceptionHandler` | 409 |

Todas responden `{ "errors": [ ... ] }`.

## Concurrencia

`Purchase` y `StockEntry` usan concurrencia optimista sobre la columna de sistema `xmin` de PostgreSQL (shadow property `RowVersion` en la configuración de EF). Si otro usuario modificó la fila entre la lectura y el guardado, EF lanza `DbUpdateConcurrencyException`, el `UnitOfWork` la traduce a `ConcurrencyException` y la API responde 409.

Esto protege lo que ocurre **dentro** de un mismo request. No cubre el caso de "abrí el formulario hace 10 minutos"; para eso habría que enviar el `RowVersion` al frontend y recibirlo al guardar.

## Relaciones entre entidades

- Los agregados se referencian **solo por Id** (`CompanyId`, `SupplierId`, `ProductId`). No hay propiedades de navegación entre agregados.
- La integridad la garantiza la base de datos con **llaves foráneas** configuradas en Persistence (`HasOne<Company>().WithMany()`), todas con `DeleteBehavior.Restrict`.
- Dentro de un agregado sí hay navegación: `Purchase.Lines`, con borrado en cascada.

## Textos y mayúsculas

PostgreSQL distingue mayúsculas en las comparaciones. Para que el comportamiento sea predecible:

- **Correos** se guardan en minúsculas (`User.NormalizeEmail`).
- **Códigos de producto** y **series** se guardan en mayúsculas (`Product.NormalizeCode`, `Purchase.NormalizeSerie`).
- Los repositorios aplican la misma normalización antes de buscar, así ninguna consulta puede olvidarla.
- Las búsquedas por nombre comparan en minúsculas en ambos lados.

## Contrato OpenAPI

El frontend (`AmirCorp-ERP-Web`) genera sus tipos desde `/openapi/v1.json`, así que el contrato tiene que ser exacto:

- Cada endpoint declara su respuesta exitosa con `[ProducesResponseType<T>(código)]`, o `[ProducesResponseType(StatusCodes.Status204NoContent)]` si no devuelve cuerpo.
- `ErrorResponseTransformer` agrega a todos los endpoints la respuesta `default` con `ErrorResponse`.
- `BearerSecurityTransformer` exige el token solo en los endpoints con `[Authorize]`.
- Los números JSON son estrictos (`JsonNumberHandling.Strict`): `"12.5"` entre comillas se rechaza.

Después de cambiar un endpoint o un DTO, en el frontend se ejecuta `npm run api:generate` y TypeScript señala lo que quedó desactualizado.

## Cómo agregar un caso de uso

Ejemplo: `Features/Products/ChangePrice`.

1. **Application/Features/Products/ChangePrice/**
   - `ChangePriceDto.cs`
   - `IChangePriceUseCase.cs` → `: IUseCase<ChangePriceDto, Result>`
   - `ChangePriceUseCase.cs` → `internal sealed`, devuelve `Result`
2. Si necesita datos nuevos, agrega el método a la interfaz en `Contracts/Persistence/...` y su implementación en `ERP.Persistence`.
3. Registra el caso de uso en `ERP.Application/DependencyInjection.cs`.
4. **Api**: crea `Controllers/Products/Requests/ChangePriceRequest.cs` con `Validate()` y `ToDto()`, y agrega el endpoint al controlador con su `[ProducesResponseType]`.
5. Si cambia el modelo de datos, genera una migración (ver [configuracion.md](configuracion.md#migraciones)).

## Convenciones de código

- Ids con `Guid.CreateVersion7()` (ordenables por fecha).
- Enums guardados como texto en la base.
- Montos con `decimal`: `(18,2)` para importes y `(18,6)` para precios unitarios, cantidades y tipos de cambio. Redondeo `MidpointRounding.AwayFromZero`.
- Fechas de negocio con `DateOnly` en hora de Lima; fechas de auditoría en UTC.
- Mensajes de error en español, dirigidos al usuario final.
