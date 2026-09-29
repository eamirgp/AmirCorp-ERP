using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <summary>
    /// "Clientes y proveedores" se divide en dos listas (Clientes y Proveedores). Las vistas guardadas que ya existían
    /// pasan a Proveedores, que es donde hoy están casi todos los registros.
    /// </summary>
    public partial class SplitPartnerSavedViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""UPDATE "SavedViews" SET "Screen" = 'Suppliers' WHERE "Screen" = 'BusinessPartners';""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""UPDATE "SavedViews" SET "Screen" = 'BusinessPartners' WHERE "Screen" IN ('Suppliers', 'Clients');""");
        }
    }
}
