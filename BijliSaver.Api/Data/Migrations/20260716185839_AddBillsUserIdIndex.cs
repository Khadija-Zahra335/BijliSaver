using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BijliSaver.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBillsUserIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_bills_user_id",
                table: "bills",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_bills_user_id",
                table: "bills");
        }
    }
}
