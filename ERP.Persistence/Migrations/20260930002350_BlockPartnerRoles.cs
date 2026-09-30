using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <summary>
    /// El "activo" general se reemplaza por un bloqueo en cada rol (compras y ventas). Quien estaba desactivado
    /// queda bloqueado en los roles que tiene, así nadie que se había dejado de usar vuelve a aparecer.
    /// </summary>
    public partial class BlockPartnerRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPurchasingBlocked",
                table: "BusinessPartners",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PurchasingBlockReason",
                table: "BusinessPartners",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSalesBlocked",
                table: "BusinessPartners",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SalesBlockReason",
                table: "BusinessPartners",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "BusinessPartners"
                SET "IsPurchasingBlocked" = "IsSupplier",
                    "IsSalesBlocked" = "IsClient"
                WHERE NOT "IsActive";
                """);

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "BusinessPartners");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "BusinessPartners",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql("""
                UPDATE "BusinessPartners"
                SET "IsActive" = FALSE
                WHERE "IsPurchasingBlocked" OR "IsSalesBlocked";
                """);

            migrationBuilder.DropColumn(
                name: "IsPurchasingBlocked",
                table: "BusinessPartners");

            migrationBuilder.DropColumn(
                name: "PurchasingBlockReason",
                table: "BusinessPartners");

            migrationBuilder.DropColumn(
                name: "IsSalesBlocked",
                table: "BusinessPartners");

            migrationBuilder.DropColumn(
                name: "SalesBlockReason",
                table: "BusinessPartners");
        }
    }
}
