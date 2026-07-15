using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BijliSaver.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedLescoTariffSlabs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // LESCO, FY 2025-26, effective 2025-07-01 per NEPRA's uniform consumer-end
            // tariff. Cross-checked against a real Dec-25 LESCO bill: 139 units billed
            // at GOP TARIFF 28.910 = the 101-200 non-protected slab below.
            // Verify upper-slab figures against nepra.org.pk before launch — secondary
            // sources differ slightly for 500+ units. The Rs 3.82/unit surcharge
            // (Mar-Jun 2026) is a separate bill line, not baked into these rates.

            migrationBuilder.Sql("""
                INSERT INTO tariff_slabs (disco_id, tariff_code, effective_from, slab_min_units, slab_max_units, rate_per_unit, is_protected)
                SELECT d.id, 'A-1', DATE '2025-07-01', s.min_u, s.max_u, s.rate, FALSE
                FROM discos d,
                (VALUES
                    (1,   100,  22.44),
                    (101, 200,  28.91),
                    (201, 300,  33.10),
                    (301, 400,  37.99),
                    (401, 500,  40.22),
                    (501, 700,  42.50),
                    (701, NULL, 47.69)
                ) AS s(min_u, max_u, rate)
                WHERE d.code = 'LESCO';
                """);

            migrationBuilder.Sql("""
                INSERT INTO tariff_slabs (disco_id, tariff_code, effective_from, slab_min_units, slab_max_units, rate_per_unit, is_protected)
                SELECT d.id, 'A-1', DATE '2025-07-01', s.min_u, s.max_u, s.rate, TRUE
                FROM discos d,
                (VALUES
                    (1,   50,   3.95),
                    (51,  100,  7.74),
                    (101, 200, 13.01)
                ) AS s(min_u, max_u, rate)
                WHERE d.code = 'LESCO';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM tariff_slabs
                USING discos d
                WHERE tariff_slabs.disco_id = d.id AND d.code = 'LESCO' AND tariff_slabs.effective_from = DATE '2025-07-01';
                """);
        }
    }
}
