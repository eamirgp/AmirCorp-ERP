using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseLineSupplierProductCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SupplierProductCode",
                table: "PurchaseLines",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            // Compras ya registradas: no se guardó el código de su factura. Se usa el que hoy tiene el producto para
            // ese proveedor, que es lo más cercano que se conoce.
            migrationBuilder.Sql("""
                UPDATE "PurchaseLines" l
                SET "SupplierProductCode" = c."Code"
                FROM "Purchases" p, "ProductSupplierCodes" c
                WHERE p."Id" = l."PurchaseId"
                  AND c."ProductId" = l."ProductId"
                  AND c."SupplierId" = p."SupplierId";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupplierProductCode",
                table: "PurchaseLines");
        }
    }
}
