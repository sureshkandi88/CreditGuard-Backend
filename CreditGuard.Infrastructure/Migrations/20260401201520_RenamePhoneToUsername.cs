using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditGuard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenamePhoneToUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Creditors",
                newName: "Username");

            migrationBuilder.RenameIndex(
                name: "IX_Creditors_Phone",
                table: "Creditors",
                newName: "IX_Creditors_Username");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Creditors",
                newName: "Phone");

            migrationBuilder.RenameIndex(
                name: "IX_Creditors_Username",
                table: "Creditors",
                newName: "IX_Creditors_Phone");
        }
    }
}
