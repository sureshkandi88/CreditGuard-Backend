using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditGuard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneToCreditor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Creditors",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Creditors_Phone",
                table: "Creditors",
                column: "Phone",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Creditors_Phone",
                table: "Creditors");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Creditors");
        }
    }
}
