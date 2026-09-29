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

`Product` también la tiene, con un paso más: la lista envía `rowVersion` y el formulario de edición la devuelve. Si otra persona (o una importación de Excel) cambió el producto después de abrir el formulario, la API responde 409 en vez de pisar el cambio sin avisar.

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
- **Números en textos** (historial, revisión de importación, mensajes): punto decimal y espacio para los miles, sin comas ("S/ 1 234.50", `NumberText`), igual que la pantalla. Así nadie confunde comas con puntos.
- **Orden por defecto:** lo decide la API (`ListProductsDto.DefaultSortBy`, etc.). La pantalla no envía orden si el usuario no eligió uno, y la respuesta informa el orden aplicado (`sortBy`, `sortDescending`) para que el menú "Ordenar" lo muestre.
- **Paginación:** cada lista paginada trae `page`, `totalPages`, `hasNextPage`, el rango visible (`from`, `to`) y los tamaños de página que se pueden elegir (`pageSizeOptions`). Un `PageSize` fuera del rango se ajusta al mínimo o al máximo, y una página que ya no existe se ajusta a la última (`PaginationDefaults.ClampPage`): la respuesta trae la página real.
- **Errores:** toda respuesta de error tiene la forma `{ errors: [...] }` con mensajes en español, también cuando un dato no se puede leer (`InvalidModelStateResponse`), en los 404 y en los errores inesperados (`UnexpectedExceptionHandler`, que deja el detalle técnico solo en el log).
- **Auditoría:** el historial de cambios se consulta en `GET /api/audit` con los textos listos para mostrar (ver la decisión 12).
- **Permisos:** qué módulos y acciones puede usar el usuario se informará en `GET /api/me` cuando existan pantallas que dependan del rol.
- **Contrato exacto:** cada endpoint declara su respuesta en OpenAPI (ver [arquitectura.md](arquitectura.md#contrato-openapi)). El frontend genera sus tipos de ahí y detecta cambios con `npm run api:check`. Las propiedades calculadas de las respuestas (`RoleDescription`, `CanImport`…) se documentan como obligatorias (`ComputedPropertiesTransformer`), porque siempre se envían.

### 11. Carga masiva de productos con Excel
**Fecha:** setiembre 2026

Los productos se pueden cargar desde un Excel (`/api/products/import/...`). Se eligió el camino más seguro:

- **Plantilla generada por la API** (`GET import/template`), con listas desplegables de unidad e IGV e instrucciones. También se pueden exportar los productos actuales (`GET export`) para editarlos y volver a subirlos. La exportación acepta los mismos filtros y orden que la lista (`SearchTerm`, `IsActive`, `SortBy`, `SortDescending`) y usa la misma consulta: el Excel trae exactamente lo que se ve. Con filtros, el archivo se llama `productos-filtrados-…`.
- **Revisión antes de guardar** (`POST import/preview`): dice fila por fila si se creará, se actualizará, se omitirá, no tiene cambios o tiene errores, con el detalle de cada cambio. No guarda nada.
- **Todo o nada** (`POST import`): vuelve a leer el archivo y, si hay un solo error, no guarda nada. Si todo está bien, guarda en una sola transacción.
- **Los códigos existentes no se tocan por defecto.** Solo se actualizan si el usuario marca "Actualizar los productos que ya existen". Así una carga de productos nuevos no puede cambiar precios por accidente.
- El producto se valida con `Product.Create`, las mismas reglas que al crearlo a mano. Límites: 5 MB y 5000 filas.
- **Precios sin comas:** si la celda es un número de Excel no hay ambigüedad. Si es texto, se acepta punto decimal y espacios para los miles; con coma la fila queda con error ("usa punto para los decimales"), sin adivinar si es de miles o decimal. La plantilla muestra los precios sin separador de miles (`0.00`).

### 12. Historial de cambios (auditoría)
**Fecha:** setiembre 2026

Cada cambio en productos, clientes y proveedores, empresas, usuarios y compras queda en la tabla `AuditLogs`: quién, cuándo, qué registro, qué acción y qué campos cambiaron (valor anterior y nuevo).

- **Automático:** lo escribe `AuditInterceptor` al guardar, en la misma transacción. Los casos de uso no hacen nada y ningún cambio se escapa, tampoco los de la carga masiva.
- **Textos listos:** `AuditDescriber` define qué campos se registran, su nombre ("Precio de venta") y cómo se muestran sus valores ("S/ 30.00", "Caja", "Sí"). Se guardan ya formateados: el historial muestra lo que el usuario vio en su momento.
- **Acciones:** creación, modificación, activación, desactivación, anulación y cambio de contraseña. De la contraseña nunca se guarda el valor.
- **Solo lectura:** la API no tiene endpoints para modificar ni borrar el historial.
- **Consulta:** `GET /api/audit` con filtros por módulo, registro, usuario, acción, rango de fechas (días en hora de Perú) y texto. Solo SuperAdmin y Admin.
- **Pantallas:** no va en las tablas ni en los formularios. Cada registro tiene su acción "Historial" (panel lateral) y hay una pantalla "Auditoría" en Administración.
- El historial empieza a registrarse desde la migración `AddAuditLogs`. Lo anterior solo conserva `CreatedBy` y `UpdatedBy` en cada tabla.

### 13. Vistas guardadas por usuario
**Fecha:** setiembre 2026

Cada usuario puede guardar con un nombre los filtros, el orden y las filas por página de una lista (tabla `SavedViews`), y marcar una por pantalla como predeterminada.

- **En la API, no en el navegador:** así el usuario tiene sus vistas en cualquier equipo. Cada usuario ve y modifica solo las suyas (`/api/saved-views`, cualquier rol).
- **Filtros opacos:** la API guarda el texto que envía la pantalla (JSON de los parámetros de la URL, máx. 2000 caracteres) y no lo interpreta. Al aplicarse, la pantalla lo valida como cualquier URL.
- **Reglas:** nombre único por usuario y pantalla (sin distinguir mayúsculas), máximo 20 por pantalla, una sola predeterminada.
- **No se audita:** es una preferencia personal, no un dato del negocio.

### 14. Código interno y códigos de proveedores
**Fecha:** setiembre 2026

Cada producto tiene **un código interno** y **varios códigos de proveedores**.

- **Código interno** (`Product.Code`): lo define la empresa, es obligatorio y único, de hasta 30 caracteres. Es el que va en la factura electrónica: en el XML UBL 2.1, el "código de producto del ítem" (`SellersItemIdentification`) es el del vendedor y admite hasta 30 caracteres. SUNAT no pide el código del proveedor en la factura ni en el registro de compras.
- **Códigos de proveedores** (tabla `ProductSupplierCodes`): el código con el que cada proveedor identifica el producto en su factura, proforma o catálogo. Uno por proveedor en cada producto, y un mismo código de un proveedor apunta a un solo producto. Así, más adelante, la factura de un proveedor se podrá relacionar con los productos sin dudas. Sirve igual para compras nacionales e importaciones.
- **No es el "Código de producto SUNAT"** (catálogo 25, 8 dígitos). Ese es obligatorio desde agosto de 2026 solo para ciertos bienes (oro, combustibles, bienes con detracción o percepción…). Se agregará con la facturación electrónica si algún producto lo necesita.
- **Búsqueda:** la lista y el buscador de las compras encuentran el producto por cualquier parte del código interno, de un código de proveedor o del nombre.
- **Formulario:** los códigos se envían junto con el producto (`SupplierCodes`) y la lista es completa: los que no vienen se quitan. Un proveedor desactivado conserva sus códigos, pero no se le agregan nuevos.
- **Historial:** agregar, cambiar o quitar un código de proveedor queda en el historial del producto ("Código de Proveedor X: — → YH-2045-BK") y cuenta como modificación del producto. Así la versión (`RowVersion`) también protege estos cambios.
- **Migración:** hasta ahora el código del producto era el del proveedor. `AddProductSupplierCodes` lo copia como código del proveedor de la última compra del producto. El código interno queda igual hasta que se cambie en el sistema.
- **Pendiente:** marca y modelo (para la DUA) y código de barras, cuando se necesiten. Los códigos de proveedores en la carga masiva con Excel.
