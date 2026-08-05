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
        /// Irradiacao global horizontal media diaria (kWh/m²/dia) das 27 UFs.
        ///
        /// Fonte: NASA POWER (Prediction of Worldwide Energy Resources), parametro
        /// ALLSKY_SFC_SW_DWN, climatologia anual. Licenca CC BY 4.0 — permite uso comercial
        /// e trabalho derivado, exigindo apenas citacao.
        ///
        /// Por que nao o CRESESB/LABREN, apesar de ser a referencia do setor no Brasil: o
        /// Atlas Brasileiro de Energia Solar e publicado sob CC BY-NC-ND, que veda uso
        /// comercial E trabalho derivado sem autorizacao expressa do INPE. Este produto e um
        /// SaaS comercial, e agregar a grade do Atlas em media por UF e um derivado — os dois
        /// pontos seriam violados. Ficou registrado como pendencia no backlog: se o INPE
        /// autorizar, vale trocar, porque o modelo BRASIL-SR e calibrado para o Brasil.
        ///
        /// Metodo: media aritmetica de 4 a 5 municipios por UF, distribuidos entre capital e
        /// interior. So a capital enviesaria o numero para baixo — a maioria das capitais
        /// brasileiras e litoranea e mais nublada que o interior do proprio estado (em SP a
        /// capital marca 4.53 e Ribeirao Preto 5.37).
        ///
        /// Reproduzivel por tools/irradiation/Build-IrradiationDataset.ps1, que lista os
        /// pontos consultados e regera este bloco.
        /// </summary>
        private static void SeedIrradiation(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO irradiation_by_uf (""Uf"", ""StateName"", ""AverageIrradiation"", ""Source"", ""UpdatedAt"") VALUES
                    ('AC', 'Acre',                4.69, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('AL', 'Alagoas',             5.75, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('AP', 'Amapá',               5.02, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('AM', 'Amazonas',            4.71, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('BA', 'Bahia',               5.50, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('CE', 'Ceará',               5.79, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('DF', 'Distrito Federal',    5.54, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('ES', 'Espírito Santo',      4.98, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('GO', 'Goiás',               5.52, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('MA', 'Maranhão',            5.40, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('MT', 'Mato Grosso',         5.31, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('MS', 'Mato Grosso do Sul',  5.19, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('MG', 'Minas Gerais',        5.30, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('PA', 'Pará',                5.03, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('PB', 'Paraíba',             5.88, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('PR', 'Paraná',              4.80, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('PE', 'Pernambuco',          5.82, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('PI', 'Piauí',               5.86, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('RJ', 'Rio de Janeiro',      4.88, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('RN', 'Rio Grande do Norte', 6.01, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('RS', 'Rio Grande do Sul',   4.67, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('RO', 'Rondônia',            4.84, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('RR', 'Roraima',             5.10, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('SC', 'Santa Catarina',      4.42, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('SP', 'São Paulo',           5.10, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('SE', 'Sergipe',             5.56, 'NASA POWER (CERES/MERRA-2)', now()),
                    ('TO', 'Tocantins',           5.41, 'NASA POWER (CERES/MERRA-2)', now())
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
