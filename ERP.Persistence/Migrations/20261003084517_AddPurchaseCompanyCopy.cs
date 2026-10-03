using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseCompanyCopy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Purchases",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyRuc",
                table: "Purchases",
                type: "character varying(11)",
                maxLength: 11,
                nullable: false,
                defaultValue: "");

            // Las compras anteriores toman el RUC y la razón social que su empresa tiene hoy: hasta ahora no se
            // podían guardar de otra forma y la empresa no ha cambiado de RUC.
            migrationBuilder.Sql("""
                UPDATE "Purchases" p
                SET "CompanyRuc" = c."Ruc", "CompanyName" = c."Name"
                FROM "Companies" c
                WHERE c."Id" = p."CompanyId";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "CompanyRuc",
                table: "Purchases");
        }
    }
}
