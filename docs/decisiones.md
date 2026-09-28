# Decisiones técnicas

Registro breve de las decisiones importantes, con su motivo. Si una decisión cambia, se agrega una nueva entrada en vez de borrar la anterior.

---

### 1. Una sola instalación con varias empresas (no multi-tenant)
**Fecha:** setiembre 2026

El sistema es para un solo grupo: el dueño tiene **3 empresas (3 RUC)**. Se descartó el modelo SaaS multi-tenant por ser más complejo de lo necesario.

- **Compartido entre las empresas:** catálogo de productos, clientes y proveedores.
- **Separado por empresa (`CompanyId`):** compras, stock, costos, y más adelante ventas, series y certificado digital. SUNAT exige llevarlos por RUC.
- Si en el futuro otro negocio usa el sistema, se le instala una copia aparte con su propia base de datos.

### 2. PostgreSQL en vez de SQL Server
**Fecha:** setiembre 2026

La producción irá en **Railway**, que ofrece PostgreSQL nativo y no SQL Server. Se cambió cuando la base estaba vacía, para que no costara nada.

Consecuencias en el código:
- Concurrencia con la columna de sistema `xmin` en vez de `rowversion`.
- PostgreSQL distingue mayúsculas: se normalizan correos, códigos y series, y las búsquedas comparan en minúsculas (ver decisión 5).

### 3. Concurrencia optimista en tablas de inventario
**Fecha:** setiembre 2026

`Purchase` y `StockEntry` tienen control de concurrencia. Evita casos como vender el último producto dos veces al mismo tiempo o anular una compra mientras se vende su mercadería. Se agregará a ventas y a las series de comprobantes cuando existan.

No se usan transacciones explícitas: cada caso de uso hace un solo `SaveChanges`, que EF Core ya ejecuta en una transacción.

### 4. Llaves foráneas en la base, sin navegación en el dominio
**Fecha:** setiembre 2026

Los agregados se referencian solo por Id para no acoplarse entre sí. La base de datos garantiza la integridad con llaves foráneas `Restrict`, configuradas únicamente en Persistence. Nada se borra físicamente: los registros se activan o desactivan.

No hay llaves foráneas de auditoría (`CreatedBy` → `Users`), porque la auditoría no debe impedir operar sobre los usuarios.

### 5. Normalización de textos en el dominio
**Fecha:** setiembre 2026

- Correos en minúsculas.
- Códigos de producto y series de comprobantes en mayúsculas (SUNAT usa series en mayúsculas).

La función de normalización vive en la entidad (`User.NormalizeEmail`, `Product.NormalizeCode`, `Purchase.NormalizeSerie`) y los repositorios la reutilizan al buscar.

### 6. Secretos fuera del repositorio
**Fecha:** setiembre 2026

User Secrets en desarrollo y variables de entorno en producción. La API se niega a arrancar si falta la conexión o la clave JWT, con un mensaje claro. Ver [configuracion.md](configuracion.md).

### 7. SuperAdmin creado al arrancar
**Fecha:** setiembre 2026

Con la base vacía no había forma de crear el primer usuario (crear usuarios exige estar autenticado). Se crea automáticamente desde la configuración si no existe ninguno. Las operaciones sin usuario autenticado se auditan con `SystemUserId` (`Guid.Empty`).

### 8. Facturación electrónica al final
**Fecha:** setiembre 2026

Primero se construye el negocio completo (importaciones, inventario, ventas). La facturación electrónica se conecta al final, pero **el modelo de ventas se diseña desde el inicio con los datos que exige SUNAT**: serie y correlativo, documento del cliente, IGV por línea, moneda, estado y fecha de emisión. Así la integración no obliga a rehacer ventas.

Reglas a respetar en ventas:
- La fecha de emisión no puede ser futura ni tener más de 3 días de antigüedad (plazo de envío a SUNAT, a confirmar con el contador).
- El periodo tributario lo define la fecha de emisión, no la de envío.

### 9. Historial reiniciado
**Fecha:** setiembre 2026

El proyecto se reinició en un repositorio nuevo (`AmirCorp-ERP`) con una sola migración inicial para PostgreSQL. El repositorio anterior (`ERP`, commits API V1 a V62) queda como archivo.

### 10. La API es la única fuente de verdad (frontend "tonto")
**Fecha:** setiembre 2026

El frontend (`AmirCorp-ERP-Web`) no valida, no calcula, no normaliza y no decide permisos. Por eso la API debe entregar todo lo necesario:

- **Validaciones completas** con mensajes listos para el usuario, en `{ "errors": [...] }`.
- **Cálculos:** cuando una pantalla necesita mostrar un resultado antes de guardar (totales de una compra, costo de una importación), se expone un endpoint de cálculo que no guarda nada. El frontend no replica fórmulas.
- **Descripciones** de enums y catálogos en las respuestas (`...Description`).
- **Permisos:** qué módulos y acciones puede usar el usuario se informará en `GET /api/me` cuando existan pantallas que dependan del rol.
- **Contrato exacto:** cada endpoint declara su respuesta en OpenAPI (ver [arquitectura.md](arquitectura.md#contrato-openapi)). El frontend genera sus tipos de ahí y detecta cambios con `npm run api:check`.
