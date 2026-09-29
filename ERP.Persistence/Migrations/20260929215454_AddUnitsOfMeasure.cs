using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitsOfMeasure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InvoiceUnitOfMeasure",
                table: "PurchaseLines",
                newName: "InvoiceUnitOfMeasureCode");

            migrationBuilder.RenameColumn(
                name: "UnitOfMeasure",
                table: "Products",
                newName: "UnitOfMeasureCode");

            migrationBuilder.CreateTable(
                name: "UnitsOfMeasure",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    SunatName = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    FixedConversionFactor = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitsOfMeasure", x => x.Id);
                    table.UniqueConstraint("AK_UnitsOfMeasure_Code", x => x.Code);
                });

            // Catálogo N.° 03 de SUNAT: las unidades de uso común con su nombre oficial (Tabla 6 del PLE). SUNAT acepta
            // cualquier código de UN/ECE Rec. 20; si se necesita otra, se agrega con una migración.
            // Factor fijo = cuántas unidades trae siempre; null = cada compra lo indica.
            migrationBuilder.Sql("""
                INSERT INTO "UnitsOfMeasure" ("Id", "Code", "SunatName", "Name", "FixedConversionFactor", "IsActive", "CreatedAt", "CreatedBy")
                SELECT gen_random_uuid(), v.code, v.sunat_name, v.name, v.factor, false, now(), '00000000-0000-0000-0000-000000000000'
                FROM (VALUES
                    ('4A',  'BOBINAS', 'Bobina', NULL::numeric),
                    ('BJ',  'BALDE', 'Balde', NULL),
                    ('BLL', 'BARRILES', 'Barril', NULL),
                    ('BG',  'BOLSA', 'Bolsa', NULL),
                    ('BO',  'BOTELLAS', 'Botella', NULL),
                    ('BX',  'CAJA', 'Caja', NULL),
                    ('CT',  'CARTONES', 'Cartón', NULL),
                    ('CMK', 'CENTIMETRO CUADRADO', 'Centímetro cuadrado', NULL),
                    ('CMQ', 'CENTIMETRO CUBICO', 'Centímetro cúbico', NULL),
                    ('CMT', 'CENTIMETRO LINEAL', 'Centímetro', NULL),
                    ('CEN', 'CIENTO DE UNIDADES', 'Ciento', 100),
                    ('CY',  'CILINDRO', 'Cilindro', NULL),
                    ('CJ',  'CONOS', 'Cono', NULL),
                    ('DZN', 'DOCENA', 'Docena', 12),
                    ('DZP', 'DOCENA POR 10**6', 'Docena por millón', NULL),
                    ('BE',  'FARDO', 'Fardo', NULL),
                    ('GLI', 'GALON INGLES (4,545956L)', 'Galón inglés', NULL),
                    ('GRM', 'GRAMO', 'Gramo', NULL),
                    ('GRO', 'GRUESA', 'Gruesa', 144),
                    ('HLT', 'HECTOLITRO', 'Hectolitro', NULL),
                    ('LEF', 'HOJA', 'Hoja', NULL),
                    ('SET', 'JUEGO', 'Juego', NULL),
                    ('KGM', 'KILOGRAMO', 'Kilogramo', NULL),
                    ('KTM', 'KILOMETRO', 'Kilómetro', NULL),
                    ('KWH', 'KILOVATIO HORA', 'Kilovatio hora', NULL),
                    ('KT',  'KIT', 'Kit', NULL),
                    ('CA',  'LATAS', 'Lata', NULL),
                    ('LBR', 'LIBRAS', 'Libra', NULL),
                    ('LTR', 'LITRO', 'Litro', NULL),
                    ('MWH', 'MEGAWATT HORA', 'Megavatio hora', NULL),
                    ('MTR', 'METRO', 'Metro', NULL),
                    ('MTK', 'METRO CUADRADO', 'Metro cuadrado', NULL),
                    ('MTQ', 'METRO CUBICO', 'Metro cúbico', NULL),
                    ('MGM', 'MILIGRAMOS', 'Miligramo', NULL),
                    ('MLT', 'MILILITRO', 'Mililitro', NULL),
                    ('MMT', 'MILIMETRO', 'Milímetro', NULL),
                    ('MMK', 'MILIMETRO CUADRADO', 'Milímetro cuadrado', NULL),
                    ('MMQ', 'MILIMETRO CUBICO', 'Milímetro cúbico', NULL),
                    ('MLL', 'MILLARES', 'Millar', 1000),
                    ('UM',  'MILLON DE UNIDADES', 'Millón de unidades', 1000000),
                    ('ONZ', 'ONZAS', 'Onza', NULL),
                    ('PF',  'PALETAS', 'Paleta', NULL),
                    ('PK',  'PAQUETE', 'Paquete', NULL),
                    ('PR',  'PAR', 'Par', 2),
                    ('FOT', 'PIES', 'Pie', NULL),
                    ('FTK', 'PIES CUADRADOS', 'Pie cuadrado', NULL),
                    ('FTQ', 'PIES CUBICOS', 'Pie cúbico', NULL),
                    ('C62', 'PIEZAS', 'Pieza', 1),
                    ('PG',  'PLACAS', 'Placa', NULL),
                    ('ST',  'PLIEGO', 'Pliego', NULL),
                    ('INH', 'PULGADAS', 'Pulgada', NULL),
                    ('RM',  'RESMA', 'Resma', NULL),
                    ('DR',  'TAMBOR', 'Tambor', NULL),
                    ('STN', 'TONELADA CORTA', 'Tonelada corta', NULL),
                    ('LTN', 'TONELADA LARGA', 'Tonelada larga', NULL),
                    ('TNE', 'TONELADAS', 'Tonelada', NULL),
                    ('TU',  'TUBOS', 'Tubo', NULL),
                    ('NIU', 'UNIDAD (BIENES)', 'Unidad', 1),
                    ('ZZ',  'UNIDAD (SERVICIOS)', 'Servicio', NULL),
                    ('GLL', 'US GALON (3,7843 L)', 'Galón', NULL),
                    ('YRD', 'YARDA', 'Yarda', NULL),
                    ('YDK', 'YARDA CUADRADA', 'Yarda cuadrada', NULL)
                ) AS v(code, sunat_name, name, factor);

                -- Activas desde el inicio: las que usa la empresa y cualquiera que ya tenga un producto o una compra.
                UPDATE "UnitsOfMeasure" SET "IsActive" = true
                WHERE "Code" IN ('NIU', 'C62', 'DZN', 'BX')
                   OR "Code" IN (SELECT "UnitOfMeasureCode" FROM "Products")
                   OR "Code" IN (SELECT "InvoiceUnitOfMeasureCode" FROM "PurchaseLines");
                """);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseLines_InvoiceUnitOfMeasureCode",
                table: "PurchaseLines",
                column: "InvoiceUnitOfMeasureCode");

            migrationBuilder.CreateIndex(
                name: "IX_Products_UnitOfMeasureCode",
                table: "Products",
                column: "UnitOfMeasureCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_UnitsOfMeasure_UnitOfMeasureCode",
                table: "Products",
                column: "UnitOfMeasureCode",
                principalTable: "UnitsOfMeasure",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseLines_UnitsOfMeasure_InvoiceUnitOfMeasureCode",
                table: "PurchaseLines",
                column: "InvoiceUnitOfMeasureCode",
                principalTable: "UnitsOfMeasure",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_UnitsOfMeasure_UnitOfMeasureCode",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseLines_UnitsOfMeasure_InvoiceUnitOfMeasureCode",
                table: "PurchaseLines");

            migrationBuilder.DropTable(
                name: "UnitsOfMeasure");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseLines_InvoiceUnitOfMeasureCode",
                table: "PurchaseLines");

            migrationBuilder.DropIndex(
                name: "IX_Products_UnitOfMeasureCode",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "InvoiceUnitOfMeasureCode",
                table: "PurchaseLines",
                newName: "InvoiceUnitOfMeasure");

            migrationBuilder.RenameColumn(
                name: "UnitOfMeasureCode",
                table: "Products",
                newName: "UnitOfMeasure");
        }
    }
}
