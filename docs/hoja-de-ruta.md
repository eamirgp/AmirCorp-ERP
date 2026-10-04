# Hoja de ruta

## Hecho

- [x] Autenticación JWT, usuarios y roles
- [x] Empresas, productos, clientes y proveedores
- [x] Compras nacionales con ingreso de stock por lotes
- [x] Concurrencia optimista (`xmin`) en compras y stock; en productos, el formulario envía la versión que abrió (`RowVersion`)
- [x] Errores siempre como `{ errors: [...] }` en español: datos con formato inválido, 404 y errores inesperados (500)
- [x] Llaves foráneas faltantes
- [x] SuperAdmin automático al arrancar
- [x] Migración a PostgreSQL
- [x] Secretos fuera del repositorio, con validación al arrancar
- [x] Normalización de correos, códigos y series
- [x] OpenAPI (`/openapi/v1.json`) y Scalar (`/scalar`) en desarrollo, con el tipo de respuesta de cada endpoint
- [x] Frontend en `AmirCorp-ERP-Web` con todas las pantallas de la API: productos, clientes y proveedores, empresas, usuarios y compras
- [x] Vista previa de compras (`POST /api/purchases/preview`): calcula montos y totales sin guardar, con la misma fórmula del dominio
- [x] Descripción del rol en la lista de usuarios y factor de conversión fijo en el catálogo de unidades
- [x] Filas por página (10, 20, 50 o 100) en las listas paginadas; 10 por defecto
- [x] Vistas guardadas por usuario, con vista predeterminada por pantalla (ver [decisiones.md](decisiones.md#13-vistas-guardadas-por-usuario))
- [x] Historial de cambios (auditoría) con consulta por registro y pantalla general (ver [decisiones.md](decisiones.md#12-historial-de-cambios-auditoría))
- [x] Unidades de medida desde el catálogo N.° 03 de SUNAT, activables por la empresa (ver [decisiones.md](decisiones.md#15-unidades-de-medida-desde-el-catálogo-de-sunat))
- [x] Código interno y códigos de proveedores por producto (ver [decisiones.md](decisiones.md#14-código-interno-y-códigos-de-proveedores))
- [x] Datos repetidos guardados al mismo tiempo (índice único): la API responde 409 en vez de 500
- [x] Carga masiva de productos con Excel: plantilla, exportación, revisión sin guardar y confirmación todo o nada (ver [decisiones.md](decisiones.md#11-carga-masiva-de-productos-con-excel))
- [x] Rediseño de la pantalla al estilo Apple, pieza por pieza: inicio de sesión, marco de la aplicación (menú lateral por área, barra superior) componentes base (botones, campos, ventanas, avisos, tabla en franjas) y la pantalla de Productos. Siguen las demás pantallas; el modo oscuro, al final (decisiones 12 a 17 de `AmirCorp-ERP-Web`; la 16 pasó los estados a los colores de Apple y la 17 las listas desplegables y los menús a los de la Mac)
- [x] Ordenar las listas por cada columna (ver [decisiones.md](decisiones.md#35-ordenar-las-listas-por-cada-columna))

## Siguiente: base técnica

| Tarea | Por qué |
|---|---|
| Costo en dólares | `PurchaseLine.InventoryUnitCost` y `StockEntry.UnitCost` quedan en la moneda de la factura, sin aplicar el tipo de cambio: el stock mezcla soles y dólares. **Resolver antes de importaciones.** |
| Logs con Serilog | Archivo de logs y consola sin el SQL de cada consulta |
| Tests del costeo | `ERP.Domain.Tests` ya prueba el IGV, la conversión de unidades y las reglas principales (72 pruebas, decisión 34); falta el costeo cuando exista el kárdex |
| Excel con los errores (opcional) | En la carga masiva, descargar el mismo archivo con una columna que explique el error de cada fila |

## Después: módulos del negocio

1. **Importaciones**
   - Proveedores extranjeros. Hoy `Purchase` exige un proveedor con RUC.
   - DUA: FOB, flete, seguro, CIF, ad valorem, IGV 16 %, IPM 2 %, percepción.
   - Gastos locales (agente de aduanas, almacén, transporte) y prorrateo.
   - Costo unitario puesto en almacén. IGV, IPM y percepción son crédito fiscal, no costo.
   - `StockEntry.PurchaseLineId` es obligatorio hoy; los lotes también nacerán de importaciones y ajustes.
2. **Inventario**: almacenes, kardex, ajustes y traslados.
3. **Ventas**: facturas, boletas y notas de crédito con series y correlativos. Consumen stock. Modelo listo para SUNAT (ver [decisiones.md](decisiones.md#8-facturación-electrónica-al-final)).
4. **Guías de remisión remitente**: por venta, traslado entre establecimientos e importación.
5. **Caja y cobranzas**: pagos, ventas al crédito, cuentas por cobrar y por pagar.
6. **Reportes y dashboard**.
7. **Facturación electrónica**: XML UBL 2.1, firma digital, envío a SUNAT u OSE, CDR, representación impresa con QR.
8. **Frontend**: repositorio aparte, `AmirCorp-ERP-Web` (React + Vite). Consume el contrato OpenAPI de esta API.
9. **Despliegue en Railway**.

## Decisiones pendientes

- **Método de costeo:** el código usa lotes FIFO (`StockEntry`). La alternativa es promedio ponderado, más simple y común en importadoras. Confirmar con el contador.
- **Precio de venta:** ¿igual en las 3 empresas o propio de cada una? Hoy está en el producto, que es compartido.
- **Facturación electrónica:** envío directo a SUNAT (certificado digital + usuario SOL) o a través de un OSE/PSE.
