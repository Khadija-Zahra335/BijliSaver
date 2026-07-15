using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BijliSaver.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bills",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    disco_id = table.Column<short>(type: "smallint", nullable: false),
                    reference_no = table.Column<string>(type: "text", nullable: true),
                    tariff_code = table.Column<string>(type: "text", nullable: true),
                    billing_month = table.Column<DateOnly>(type: "date", nullable: false),
                    units_consumed = table.Column<int>(type: "integer", nullable: true),
                    current_bill = table.Column<decimal>(type: "numeric", nullable: true),
                    total_fpa = table.Column<decimal>(type: "numeric", nullable: true),
                    arrears = table.Column<decimal>(type: "numeric", nullable: true),
                    payable_within_due = table.Column<decimal>(type: "numeric", nullable: true),
                    payable_after_due = table.Column<decimal>(type: "numeric", nullable: true),
                    lp_surcharge = table.Column<decimal>(type: "numeric", nullable: true),
                    fpa_rate = table.Column<decimal>(type: "numeric", nullable: true),
                    fpa_month = table.Column<DateOnly>(type: "date", nullable: true),
                    gop_rate = table.Column<decimal>(type: "numeric", nullable: true),
                    reading_is_estimated = table.Column<bool>(type: "boolean", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    ocr_status = table.Column<string>(type: "text", nullable: false),
                    ocr_confidence = table.Column<decimal>(type: "numeric", nullable: true),
                    ocr_raw = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bills", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "charge_definitions",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "text", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    explanation_en = table.Column<string>(type: "text", nullable: false),
                    explanation_ur = table.Column<string>(type: "text", nullable: true),
                    category = table.Column<string>(type: "text", nullable: false),
                    can_be_negative = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_charge_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "discos",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tariff_slabs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    disco_id = table.Column<short>(type: "smallint", nullable: false),
                    tariff_code = table.Column<string>(type: "text", nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    slab_min_units = table.Column<int>(type: "integer", nullable: false),
                    slab_max_units = table.Column<int>(type: "integer", nullable: true),
                    rate_per_unit = table.Column<decimal>(type: "numeric", nullable: false),
                    is_protected = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tariff_slabs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bill_history_entries",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    month = table.Column<DateOnly>(type: "date", nullable: false),
                    units = table.Column<int>(type: "integer", nullable: true),
                    amount = table.Column<decimal>(type: "numeric", nullable: true),
                    is_estimated = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bill_history_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_bill_history_entries_bills_bill_id",
                        column: x => x.bill_id,
                        principalTable: "bills",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bill_insights",
                columns: table => new
                {
                    bill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    slab_reached = table.Column<int>(type: "integer", nullable: true),
                    units_to_next_slab = table.Column<int>(type: "integer", nullable: true),
                    next_slab_penalty = table.Column<decimal>(type: "numeric", nullable: true),
                    tax_total = table.Column<decimal>(type: "numeric", nullable: true),
                    tax_percentage = table.Column<decimal>(type: "numeric", nullable: true),
                    energy_total = table.Column<decimal>(type: "numeric", nullable: true),
                    has_arrears = table.Column<bool>(type: "boolean", nullable: false),
                    appliance_summary = table.Column<string>(type: "text", nullable: true),
                    predicted_next_amt = table.Column<decimal>(type: "numeric", nullable: true),
                    prediction_basis = table.Column<string>(type: "text", nullable: true),
                    savings_tip = table.Column<string>(type: "text", nullable: true),
                    computed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bill_insights", x => x.bill_id);
                    table.ForeignKey(
                        name: "FK_bill_insights_bills_bill_id",
                        column: x => x.bill_id,
                        principalTable: "bills",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bill_charges",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    charge_def_id = table.Column<short>(type: "smallint", nullable: true),
                    raw_label = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bill_charges", x => x.id);
                    table.ForeignKey(
                        name: "FK_bill_charges_bills_bill_id",
                        column: x => x.bill_id,
                        principalTable: "bills",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bill_charges_charge_definitions_charge_def_id",
                        column: x => x.charge_def_id,
                        principalTable: "charge_definitions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_bill_charges_bill_id",
                table: "bill_charges",
                column: "bill_id");

            migrationBuilder.CreateIndex(
                name: "IX_bill_charges_charge_def_id",
                table: "bill_charges",
                column: "charge_def_id");

            migrationBuilder.CreateIndex(
                name: "IX_bill_history_entries_bill_id",
                table: "bill_history_entries",
                column: "bill_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bill_charges");

            migrationBuilder.DropTable(
                name: "bill_history_entries");

            migrationBuilder.DropTable(
                name: "bill_insights");

            migrationBuilder.DropTable(
                name: "discos");

            migrationBuilder.DropTable(
                name: "tariff_slabs");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "charge_definitions");

            migrationBuilder.DropTable(
                name: "bills");
        }
    }
}
