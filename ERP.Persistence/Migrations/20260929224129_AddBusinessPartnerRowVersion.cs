using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessPartnerRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "BusinessPartners",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            // Los registros que ya existen quedan como los guarda ahora el dominio: documento sin espacios y en
            // mayúsculas, y nombre sin espacios de sobra.
            migrationBuilder.Sql("""
                UPDATE "BusinessPartners"
                SET "DocumentNumber" = upper(regexp_replace("DocumentNumber", '\s', '', 'g')),
                    "Name" = regexp_replace(btrim("Name"), '\s+', ' ', 'g');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                table: "BusinessPartners");
        }
    }
}
