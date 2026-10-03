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
- PostgreSQL distingue mayúsculas: se normalizan correos, códigos y series, y las búsquedas comparan en minúsculas (ver decisión 5). Los nombres también se comparan sin tildes con la extensión `unaccent` ("camara" encuentra "Cámara"; `SearchText.Normalize` prepara el texto buscado igual).

### 3. Concurrencia optimista en tablas de inventario
**Fecha:** setiembre 2026

`Purchase` y `StockEntry` tienen control de concurrencia. Evita casos como vender el último producto dos veces al mismo tiempo o anular una compra mientras se vende su mercadería. Se agregará a ventas y a las series de comprobantes cuando existan.

`Product` también la tiene, con un paso más: la lista envía `rowVersion` y el formulario de edición la devuelve. Si otra persona (o una importación de Excel) cambió el producto después de abrir el formulario, la API responde 409 en vez de pisar el cambio sin avisar.

No se usan transacciones explícitas: cada caso de uso hace un solo `SaveChanges`, que EF Core ya ejecuta en una transacción.

### 4. Llaves foráneas en la base, sin navegación en el dominio
**Fecha:** setiembre 2026

Los agregados se referencian solo por Id para no acoplarse entre sí. La base de datos garantiza la integridad con llaves foráneas `Restrict`, configuradas únicamente en Persistence. Nada se borra físicamente: los registros se activan o desactivan (clientes y proveedores, en cambio, se bloquean por rol: ver la decisión 16).

**Excepción pendiente de corregir:** hoy anular una compra borra sus entradas de stock (`StockEntry`). Se cambiará por un movimiento que las revierta, para que el kárdex conserve lo que pasó.

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
- **Búsqueda:** la lista de productos encuentra el producto por cualquier parte del código interno, de un código de proveedor o del nombre. En una compra, la primera búsqueda es solo entre los productos enlazados a ese proveedor (ver la decisión 17).
- **Dónde se ven** (como en Odoo o Business Central): en la ficha del producto. La lista no los muestra; solo cuando el producto salió por un código de proveedor, la API envía `SearchMatch` ("Encontrado por el código YH-2045-BK de Proveedor X"). En una compra, la búsqueda envía el proveedor (`SupplierId`) y cada producto trae el código de ese proveedor (`SupplierCode`) para compararlo con su factura.
- **Formulario:** los códigos se envían junto con el producto (`SupplierCodes`) y la lista es completa: los que no vienen se quitan. Un proveedor con compras bloqueadas conserva sus códigos, pero no se le agregan nuevos.
- **Historial:** agregar, cambiar o quitar un código de proveedor queda en el historial del producto ("Código de Proveedor X: — → YH-2045-BK") y cuenta como modificación del producto. Así la versión (`RowVersion`) también protege estos cambios.
- **Migración:** hasta ahora el código del producto era el del proveedor. `AddProductSupplierCodes` lo copia como código del proveedor de la última compra del producto. El código interno queda igual hasta que se cambie en el sistema.
- **Pendiente:** marca y modelo (para la DUA) y código de barras, cuando se necesiten. Los códigos de proveedores en la carga masiva con Excel.

### 15. Unidades de medida desde el catálogo de SUNAT
**Fecha:** setiembre 2026

Las unidades de medida están en una tabla (`UnitsOfMeasure`), no fijas en el código.

- **Qué dice SUNAT:** el catálogo N.° 03 del Anexo N.° 8 no es una lista propia, sino el estándar internacional "UN/ECE Recommendation 20" (más de 2000 códigos). Si la factura lleva un código que no está en ese estándar, SUNAT la rechaza.
- **Qué se carga:** la migración `AddUnitsOfMeasure` carga las 62 unidades de uso común con su nombre oficial (la lista de la Tabla 6 del PLE: unidad, pieza, docena, caja, par, juego, kit, metro, litro…). Si algún día se necesita otro código del estándar, se agrega con una migración. **No se crean desde el sistema**, para no inventar códigos que SUNAT rechace.
- **Qué elige la empresa:** en Administración › Unidades de medida activa las que usa (al inicio: Unidad, Pieza, Docena y Caja) y puede darles un nombre corto ("Unidad" en vez de "UNIDAD (BIENES)"). En productos, compras y la planilla de Excel solo aparecen las activas (`GET api/catalogs/units-of-measure`).
- **Protección:** una unidad que usa algún producto no se puede desactivar (409, con cuántos productos la usan).
- **Código SUNAT como llave:** productos (`UnitOfMeasureCode`) y líneas de compra (`InvoiceUnitOfMeasureCode`) guardan el código, que es el dato que va en la factura, con llave foránea a `UnitsOfMeasure.Code`.
- **Factor fijo:** las unidades que siempre traen lo mismo (Docena 12, Par 2, Ciento 100, Millar 1000, Gruesa 144) fijan el factor de conversión en las compras. Las demás (Caja, Paquete, Kilogramo…) lo indica cada compra.
- **Excel:** la columna "Unidad de medida" acepta el nombre corto, el nombre SUNAT o el código, sin distinguir mayúsculas ni tildes.
- **Historial:** activar, desactivar o renombrar una unidad queda en su historial. En el de un producto, el cambio de unidad se muestra con nombres ("Unidad → Docena").

### 16. Clientes y proveedores: documento verificado y protegido
**Fecha:** setiembre 2026

- **RUC verificado como SUNAT:** 11 dígitos, prefijo 10, 15, 16, 17 o 20 y dígito verificador (módulo 11). DNI de 8 dígitos. Documento extranjero de hasta 20 letras, números o guiones. Una sola regla (`DocumentNumberError`) para la API y el dominio.
- **Nombre de hasta 200 caracteres:** las razones sociales de SUNAT (consorcios, asociaciones) pueden pasar de 100; lo mismo vale para las empresas propias y para la copia que guarda cada compra (migración `LongerLegalNames`). Al editar, la consulta a SUNAT o RENIEC sirve para actualizar el nombre y ver el estado actual del contribuyente.
- **Normalización:** el documento se guarda sin espacios y en mayúsculas, y el nombre sin espacios de sobra. El duplicado se compara ya normalizado y el mensaje dice quién lo tiene ("Ya existe ACME S.A.C. con RUC 20123456789.").
- **País:** con DNI o RUC es siempre Perú y no se pregunta. Con documento extranjero se elige, y no puede ser Perú. El catálogo de tipos de documento indica `RequiresCountry` para que la pantalla sepa cuándo mostrar el campo.
- **Catálogo de países:** los 249 países del catálogo N.° 04 de SUNAT (ISO 3166-1, dos letras) con su nombre en español, en código (`Countries`), sin tabla ni pantalla: son datos fijos que no se configuran. El cliente o proveedor guarda el código (`CountryCode`). `GET api/catalogs/countries` los devuelve sin Perú y ordenados por nombre; el formulario los busca escribiendo. Servirá también para el país de origen de la DUA.
- **El documento identifica al contribuyente:** mientras no tenga compras se puede corregir (error de tipeo). Con compras ya no, porque el registro mezclaría dos empresas; si es otra empresa, se registra como nueva. Cada compra guarda su propia copia del RUC y la razón social, así que el historial nunca cambia. La razón social sí se puede editar siempre.
- **Roles sin casillas:** el formulario no tiene casillas de rol, como Odoo o Business Central. El rol lo da la lista donde se crea ("Nuevo cliente" o "Nuevo proveedor"). Para que alguien sea las dos cosas, se usa la acción "Registrar también como…" del menú ⋯ (`PATCH api/partners/{id}/roles/{role}`), que valida que el documento sirva para ese rol. Al crear, si el documento ya existe en la otra lista, el formulario lo detecta (`GET api/partners/by-document`) y ofrece agregarlo también a esta en vez de duplicarlo. Los roles no se quitan: si ya no se le compra o vende, se bloquea ese rol.
- **Documento según el rol:** un proveedor necesita RUC o documento extranjero (con DNI no emite facturas). Por ahora la empresa **solo vende en Perú**, así que un cliente necesita RUC o DNI (`CanBeClient`; cuando haya exportaciones, se agrega ahí el documento extranjero). Cliente y proveedor a la vez, entonces, solo con RUC. El dominio y la API lo validan; el catálogo de tipos de documento informa `CanBeSupplier` y `CanBeClient` para que la pantalla no ofrezca combinaciones imposibles.
- **Concurrencia:** el formulario envía `RowVersion` (xmin), como en productos.
- **Bloqueo por rol, sin "desactivar":** como el Business Partner de SAP, pero más simple. Un registro no se desactiva entero: se bloquean sus **compras** (rol proveedor, `IsPurchasingBlocked`) o sus **ventas** (rol cliente, `IsSalesBlocked`), cada uno con un motivo opcional de hasta 200 caracteres (`PATCH api/partners/{id}/roles/{role}/block` y `/unblock`). Dejar de comprarle a alguien no impide seguir vendiéndole. Una compra nueva rechaza al proveedor con compras bloqueadas y dice el motivo; las compras hechas no cambian. Cada lista muestra el estado de su rol ("Activo", "Compras bloqueadas", "Ventas bloqueadas") y su filtro `IsBlocked` mira el bloqueo de ese rol. El historial registra el bloqueo y su motivo. Al migrar (`BlockPartnerRoles`), quien estaba desactivado quedó bloqueado en los roles que tenía.
- **Un registro, dos listas:** por dentro hay un solo registro por contribuyente (una empresa puede ser cliente y proveedor, como el "Business Partner" de SAP o el contacto de Odoo). En la pantalla se ven dos listas: **Clientes** (en Comercial) y **Proveedores** (en Abastecimiento), cada una filtrada por su rol y con sus propias vistas guardadas (`SavedViewScreen.Clients` y `Suppliers`; migración `SplitPartnerSavedViews`). En Proveedores no se ofrece el filtro por DNI (`CanBeSupplier` en los tipos de documento).
- **Consulta de RUC en SUNAT y de DNI en RENIEC:** `GET api/partners/document-lookup?identityDocumentType=&documentNumber=`. Con RUC devuelve razón social, estado, condición y dirección fiscal, con avisos si el contribuyente no está ACTIVO y HABIDO o si ya está registrado. Con DNI devuelve apellidos y nombres (el orden que usa SUNAT para personas). El catálogo de tipos de documento informa `LookupSource` ("SUNAT" o "RENIEC") para el texto del botón. La hace la API (el token nunca llega al navegador) con un proveedor intercambiable (`IRucLookup`); hoy [Decolecta](https://decolecta.com), con plan gratuito de 1000 consultas al mes. Sin `RucLookup:Token` la consulta no está disponible (`SupportsLookup` en los tipos de documento) y todo funciona a mano. Al crear, el formulario primero revisa si el documento ya existe (gratis, en la base propia): si está en la misma lista ofrece abrirlo y desactiva la consulta; si es nuevo, la consulta la pide el usuario con el botón (SUNAT o RENIEC) o con Enter en el número. No es automática a propósito: cada consulta cuenta en el cupo del servicio y un DNI no se puede validar antes (cualquier número de 8 dígitos se consultaría). Si el número cambia después de consultar, el nombre consultado se borra. Alternativa oficial y gratuita para producción: el padrón reducido del RUC que SUNAT publica a diario.

### 17. Compras: proveedor, productos y tipo de cambio desde la factura
**Fecha:** octubre 2026

La compra se registra copiando la factura del proveedor, sin tener que ir antes a otras pantallas.

- **Proveedor nuevo desde la compra:** en "Nueva compra" se busca al proveedor por RUC o razón social; si no existe, se escribe su RUC y se presiona SUNAT (o Enter). La compra viaja con `NewSupplier` (RUC y razón social) en vez de `SupplierId`, y la API registra al proveedor y la compra en la misma transacción: si la compra no se guarda, el proveedor tampoco. Se valida igual que en su pantalla. Si el RUC ya existía solo como cliente, se usa ese registro y se le agrega el rol de proveedor. Los proveedores con compras bloqueadas aparecen en el buscador con su motivo, sin poder elegirse.
- **Primero el proveedor:** las líneas se habilitan después de elegirlo, porque el código de la factura solo significa algo para ese proveedor.
- **Buscador de la línea:** muestra solo los productos enlazados a ese proveedor (`OnlySupplierProducts` en la lista de productos), por su código, el código interno o el nombre. Un producto va en una sola línea: el que ya está en otra se ve, pero no se elige.
- **"¿Lo tengo o es nuevo?" se decide una sola vez, al principio:** si lo escrito no es el código de un producto de ese proveedor, la lista ofrece al final:
  - **"Enlazar X a un producto que ya tengo":** un mismo producto puede venderlo más de un proveedor, cada uno con su código. Se busca el producto en todo el catálogo por nombre o código interno, y la línea viaja con `SupplierCode`. Con el producto elegido, para cambiarlo se escribe de nuevo en el mismo campo (sigue buscando en todo el catálogo y conserva el código); "Quitar enlace" deja la línea como al inicio. El enlace se guarda al registrar la compra (`Product.AddSupplierCode`) y la próxima compra a ese proveedor lo encuentra por ese código. Un producto que ya tiene otro código de ese proveedor no se puede elegir: se corrige en Productos.
  - **"Crear producto nuevo con código X":** la línea viaja con `NewProduct` (código interno, nombre y código del proveedor) en vez de `ProductId`, y la API lo registra con la compra en la misma transacción. El código de la factura queda como código de ese proveedor y, por defecto, también como código interno (la pantalla avisa si ese código interno ya lo usa otro producto). La afectación al IGV sale de la línea y el producto se cuenta siempre en unidades (`UnitOfMeasure.BaseUnitCode`, NIU). Nace con precio de venta 0: Ventas deberá impedir o avisar la venta de un producto sin precio.
- **La línea guarda los dos códigos:** el producto (con copia de su código interno y nombre, que es lo que mueve el inventario) y el código del proveedor tal como venía en la factura (`SupplierProductCode`), como el "número de material del proveedor" de SAP o la referencia del proveedor de Business Central. Se llena solo con el enlace del producto a ese proveedor: el que ya tenía, el recién enlazado o el del producto nuevo; queda vacío si la factura no trae código. Es una copia: si después se corrige el enlace en Productos, la compra sigue mostrando lo que decía su comprobante. Sirve para comparar con la factura y para reclamar o devolver al proveedor, que solo conoce su código. Un mismo código no puede ir en dos líneas (lo valida el dominio). Las compras anteriores a la migración `AddPurchaseLineSupplierProductCode` tomaron el código que el producto tenía en ese momento. El detalle de la compra muestra los dos códigos y lo que entró al inventario ("120 und. (24 por caja) · costo 5.00 c/u", `InventoryDescription`).
- **Unidades por caja:** el stock se cuenta en unidades. Si la factura viene en una unidad de cantidad fija (Docena, Par…), el dominio pone el factor y la pantalla no lo pregunta. Si viene en una unidad variable (Caja, Paquete…), la línea pide "Unidades por caja", porque una caja del mismo producto puede traer cantidades distintas en cada compra (ver la decisión 15).
- **Tipo de cambio de SUNAT:** en dólares, el botón SUNAT trae el tipo de cambio **venta** publicado para la fecha de emisión (`GET api/catalogs/exchange-rate?currency=&date=`), con el mismo proveedor y clave de la consulta de RUC. Se usa el de venta y de la fecha de emisión porque así lo fija el Reglamento del IGV (art. 5, num. 17); si ese día no se publicó (fin de semana, feriado), aplica el último publicado. Es a pedido, como las demás consultas. Lo consultado se guarda en la tabla `ExchangeRates` (migración `AddExchangeRates`): primero se busca ahí y solo si falta se consulta al servicio, que trae el mes completo si lo ofrece. Un día pasado sin publicación se guarda con el último publicado; el de hoy no, porque puede publicarse más tarde. Con `storedOnly=true` el endpoint responde solo lo ya guardado (404 si falta), sin consultar al servicio: la pantalla lo usa para llenar el campo sola al elegir la moneda o la fecha, sin gastar consultas; si no está guardado, lo pide el usuario con el botón. Al cambiar la fecha, el valor que vino de SUNAT se reemplaza o se borra; el escrito a mano se respeta, con un aviso para revisarlo. En importaciones la regla es otra (fecha de pago del impuesto) y se verá en ese módulo. En soles el campo queda bloqueado (`RequiresExchangeRate` y `SupportsExchangeRateLookup` en el catálogo de monedas).
- **Validaciones:**
  - La serie debe corresponder al comprobante: factura F o E, boleta B o EB, o numérica si es física.
  - El proveedor no puede tener el RUC de la empresa que compra.
  - El código interno y el código del proveedor de un producto nuevo no pueden ser de otro producto.
- **Pendiente:**
  - Facturas sin código de producto.
  - Importar el XML de la factura.
  - El costo en soles de las compras en dólares y el IGV de las boletas como costo (a revisar con el contador).
