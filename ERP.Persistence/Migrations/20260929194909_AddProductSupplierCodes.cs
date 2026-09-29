using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSupplierCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                table: "PurchaseLines",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Products",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateTable(
                name: "ProductSupplierCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSupplierCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductSupplierCodes_BusinessPartners_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "BusinessPartners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductSupplierCodes_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductSupplierCodes_ProductId_SupplierId",
                table: "ProductSupplierCodes",
                columns: new[] { "ProductId", "SupplierId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductSupplierCodes_SupplierId_Code",
                table: "ProductSupplierCodes",
                columns: new[] { "SupplierId", "Code" },
                unique: true);

            // Hasta ahora el código del producto era el del proveedor. Se copia como código del proveedor de la
            // última compra del producto; el código interno se mantiene igual hasta que se cambie en el sistema.
            // Los productos sin compras no tienen a qué proveedor asignarlo y quedan solo con su código interno.
            migrationBuilder.Sql("""
                INSERT INTO "ProductSupplierCodes" ("Id", "ProductId", "SupplierId", "Code")
                SELECT gen_random_uuid(), p."Id", last_purchase."SupplierId", upper(trim(p."Code"))
                FROM "Products" p
                JOIN LATERAL (
                    SELECT pu."SupplierId"
                    FROM "PurchaseLines" pl
                    JOIN "Purchases" pu ON pu."Id" = pl."PurchaseId"
                    WHERE pl."ProductId" = p."Id"
                    ORDER BY pu."IssueDate" DESC, pu."CreatedAt" DESC
                    LIMIT 1
                ) last_purchase ON true;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductSupplierCodes");

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                table: "PurchaseLines",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Products",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);
        }
    }
}
