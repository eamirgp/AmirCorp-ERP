using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PurchaseDocumentUniqueAcrossCompanies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Purchases_CompanyId_TaxDocumentType_SupplierId_Serie_Number",
                table: "Purchases");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_CompanyId",
                table: "Purchases",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_TaxDocumentType_SupplierId_Serie_Number",
                table: "Purchases",
                columns: new[] { "TaxDocumentType", "SupplierId", "Serie", "Number" },
                unique: true,
                filter: "\"IsCancelled\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Purchases_CompanyId",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_TaxDocumentType_SupplierId_Serie_Number",
                table: "Purchases");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_CompanyId_TaxDocumentType_SupplierId_Serie_Number",
                table: "Purchases",
                columns: new[] { "CompanyId", "TaxDocumentType", "SupplierId", "Serie", "Number" },
                unique: true,
                filter: "\"IsCancelled\" = false");
        }
    }
}
