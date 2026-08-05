using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDimensioningReferenceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StateName",
                table: "irradiation_by_uf",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "irradiation_by_uf",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "consumption_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NumRooms = table.Column<int>(type: "integer", nullable: true),
                    HasAc = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    HasWaterHeater = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    HasPool = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    StateGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ConsumptionMin = table.Column<int>(type: "integer", nullable: false),
                    ConsumptionMax = table.Column<int>(type: "integer", nullable: false),
                    ConsumptionAvg = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consumption_profiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_consumption_profiles_PropertyType_StateGroup",
                table: "consumption_profiles",
                columns: new[] { "PropertyType", "StateGroup" });

            SeedIrradiation(migrationBuilder);
            SeedConsumptionProfiles(migrationBuilder);
        }

        /// <summary>
        /// Irradiacao solar media diaria (kWh/m²/dia) das 27 UFs.
        /// Fonte: CRESESB — Atlas Brasileiro de Energia Solar (ADR-004, dataset estatico).
        /// </summary>
        private static void SeedIrradiation(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO irradiation_by_uf (""Uf"", ""StateName"", ""AverageIrradiation"", ""Source"", ""UpdatedAt"") VALUES
                    ('AC', 'Acre',                5.18, 'CRESESB 2024', now()),
                    ('AL', 'Alagoas',             5.92, 'CRESESB 2024', now()),
                    ('AP', 'Amapá',               5.55, 'CRESESB 2024', now()),
                    ('AM', 'Amazonas',            4.82, 'CRESESB 2024', now()),
                    ('BA', 'Bahia',               5.98, 'CRESESB 2024', now()),
                    ('CE', 'Ceará',               6.12, 'CRESESB 2024', now()),
                    ('DF', 'Distrito Federal',    5.48, 'CRESESB 2024', now()),
                    ('ES', 'Espírito Santo',      5.22, 'CRESESB 2024', now()),
                    ('GO', 'Goiás',               5.58, 'CRESESB 2024', now()),
                    ('MA', 'Maranhão',            5.88, 'CRESESB 2024', now()),
                    ('MT', 'Mato Grosso',         5.42, 'CRESESB 2024', now()),
                    ('MS', 'Mato Grosso do Sul',  5.12, 'CRESESB 2024', now()),
                    ('MG', 'Minas Gerais',        5.35, 'CRESESB 2024', now()),
                    ('PA', 'Pará',                5.28, 'CRESESB 2024', now()),
                    ('PB', 'Paraíba',             6.08, 'CRESESB 2024', now()),
                    ('PR', 'Paraná',              4.68, 'CRESESB 2024', now()),
                    ('PE', 'Pernambuco',          6.15, 'CRESESB 2024', now()),
                    ('PI', 'Piauí',               5.92, 'CRESESB 2024', now()),
                    ('RJ', 'Rio de Janeiro',      5.02, 'CRESESB 2024', now()),
                    ('RN', 'Rio Grande do Norte', 6.22, 'CRESESB 2024', now()),
                    ('RS', 'Rio Grande do Sul',   4.52, 'CRESESB 2024', now()),
                    ('RO', 'Rondônia',            5.08, 'CRESESB 2024', now()),
                    ('RR', 'Roraima',             5.45, 'CRESESB 2024', now()),
                    ('SC', 'Santa Catarina',      4.65, 'CRESESB 2024', now()),
                    ('SP', 'São Paulo',           4.82, 'CRESESB 2024', now()),
                    ('SE', 'Sergipe',             6.05, 'CRESESB 2024', now()),
                    ('TO', 'Tocantins',           5.68, 'CRESESB 2024', now())
                ON CONFLICT (""Uf"") DO UPDATE SET
                    ""StateName"" = EXCLUDED.""StateName"",
                    ""AverageIrradiation"" = EXCLUDED.""AverageIrradiation"",
                    ""Source"" = EXCLUDED.""Source"",
                    ""UpdatedAt"" = EXCLUDED.""UpdatedAt"";
            ");
        }

        /// <summary>
        /// Perfis de consumo do Sudeste, conforme documentado no EP-03.
        /// As demais regioes ainda nao tem dataset proprio — ate terem, a consulta cai no
        /// Sudeste e sinaliza a aproximacao na resposta.
        /// </summary>
        private static void SeedConsumptionProfiles(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO consumption_profiles
                    (""Id"", ""PropertyType"", ""NumRooms"", ""HasAc"", ""HasWaterHeater"", ""HasPool"",
                     ""StateGroup"", ""ConsumptionMin"", ""ConsumptionMax"", ""ConsumptionAvg"", ""CreatedAt"")
                VALUES
                    (gen_random_uuid(), 'Apartment',  1,   false, false, false, 'Southeast',  120,  180,  150, now()),
                    (gen_random_uuid(), 'Apartment',  2,   false, false, false, 'Southeast',  180,  280,  230, now()),
                    (gen_random_uuid(), 'Apartment',  2,   true,  false, false, 'Southeast',  350,  500,  420, now()),
                    (gen_random_uuid(), 'Apartment',  3,   false, false, false, 'Southeast',  250,  380,  310, now()),
                    (gen_random_uuid(), 'Apartment',  3,   true,  false, false, 'Southeast',  450,  650,  550, now()),
                    (gen_random_uuid(), 'House',      2,   false, false, false, 'Southeast',  200,  320,  260, now()),
                    (gen_random_uuid(), 'House',      3,   false, false, false, 'Southeast',  300,  450,  370, now()),
                    (gen_random_uuid(), 'House',      3,   true,  false, false, 'Southeast',  500,  750,  620, now()),
                    (gen_random_uuid(), 'House',      4,   true,  true,  false, 'Southeast',  800, 1200, 1000, now()),
                    (gen_random_uuid(), 'House',      4,   true,  true,  true,  'Southeast', 1200, 1800, 1500, now()),
                    (gen_random_uuid(), 'Commercial', NULL, false, false, false, 'Southeast',  500, 1000,  750, now()),
                    (gen_random_uuid(), 'Commercial', NULL, true,  false, false, 'Southeast', 1500, 3000, 2200, now());
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "consumption_profiles");

            migrationBuilder.DropColumn(
                name: "StateName",
                table: "irradiation_by_uf");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "irradiation_by_uf");
        }
    }
}
