using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BijliSaver.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedReferenceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "discos",
                columns: new[] { "id", "code", "name", "is_active" },
                values: new object[,]
                {
                    { (short)1, "LESCO", "Lahore Electric Supply Company", true },
                    { (short)2, "MEPCO", "Multan Electric Power Company", true },
                    { (short)3, "FESCO", "Faisalabad Electric Supply Company", false },
                    { (short)4, "GEPCO", "Gujranwala Electric Power Company", false },
                    { (short)5, "IESCO", "Islamabad Electric Supply Company", false },
                });

            migrationBuilder.InsertData(
                table: "charge_definitions",
                columns: new[] { "id", "code", "display_name", "explanation_en", "explanation_ur", "category", "can_be_negative", "sort_order" },
                values: new object[,]
                {
                    { (short)1, "ENERGY", "Cost of Electricity",
                        "The actual electricity you used, priced according to your tariff slab.", null,
                        "energy", false, (short)1 },
                    { (short)2, "FPA", "Fuel Price Adjustment",
                        "An extra charge added when the fuel used to generate electricity gets more expensive — or a discount when it gets cheaper.", null,
                        "adjustment", true, (short)2 },
                    { (short)3, "FC_SURCHARGE", "F.C. Surcharge",
                        "A surcharge related to fuel cost, set by the government.", null,
                        "surcharge", false, (short)3 },
                    { (short)4, "QTR_ADJ", "Quarterly Tariff Adjustment",
                        "A periodic rate adjustment approved by regulators, applied once a quarter.", null,
                        "adjustment", true, (short)4 },
                    { (short)5, "METER_RENT", "Meter Rent",
                        "A fixed monthly fee for the electricity meter installed at your home.", null,
                        "fee", false, (short)5 },
                    { (short)6, "SERVICE_RENT", "Service Rent",
                        "A fixed monthly fee for maintaining the service connection to your home.", null,
                        "fee", false, (short)6 },
                    { (short)7, "ED", "Electricity Duty",
                        "A provincial government tax charged on your electricity usage.", null,
                        "tax", false, (short)7 },
                    { (short)8, "TVFEE", "TV Fee",
                        "A fixed government fee for owning a television, collected through the electricity bill.", null,
                        "fee", false, (short)8 },
                    { (short)9, "GST", "General Sales Tax",
                        "A federal sales tax charged on your electricity bill — like the tax on shopping.", null,
                        "tax", false, (short)9 },
                    { (short)10, "INCOME_TAX", "Income Tax",
                        "Tax withheld against your income tax, collected in advance through your bill.", null,
                        "tax", false, (short)10 },
                    { (short)11, "EXTRA_TAX", "Extra Tax",
                        "An additional federal tax charged to consumers not registered as tax filers.", null,
                        "tax", false, (short)11 },
                    { (short)12, "FURTHER_TAX", "Further Tax",
                        "An additional sales tax charged to unregistered businesses.", null,
                        "tax", false, (short)12 },
                    { (short)13, "RS_TAX", "Retailers Tax",
                        "A tax collected from retail-tariff consumers on behalf of the tax authority.", null,
                        "tax", false, (short)13 },
                    { (short)14, "GST_ON_FPA", "GST on Fuel Adjustment",
                        "Sales tax charged on top of the fuel price adjustment amount.", null,
                        "tax", false, (short)14 },
                    { (short)15, "ED_ON_FPA", "Electricity Duty on Fuel Adjustment",
                        "Electricity duty charged on top of the fuel price adjustment amount.", null,
                        "tax", false, (short)15 },
                    { (short)16, "OTHER_FPA_TAX", "Other Taxes on Fuel Adjustment",
                        "Other smaller taxes charged on top of the fuel price adjustment.", null,
                        "tax", false, (short)16 },
                    { (short)17, "LP_SURCHARGE", "Late Payment Surcharge",
                        "A penalty charged for paying after the due date.", null,
                        "penalty", false, (short)17 },
                    { (short)18, "ARREARS", "Arrears",
                        "Unpaid balance carried over from previous bills — not new usage or taxes.", null,
                        "adjustment", false, (short)18 },
                    { (short)19, "DEFERRED", "Deferred Amount",
                        "An amount moved to a future bill under an approved payment plan.", null,
                        "adjustment", true, (short)19 },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM charge_definitions;");
            migrationBuilder.Sql("DELETE FROM discos;");
        }
    }
}
