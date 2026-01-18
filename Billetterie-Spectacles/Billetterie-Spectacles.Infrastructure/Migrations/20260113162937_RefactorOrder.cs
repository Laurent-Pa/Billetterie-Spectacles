using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billetterie_Spectacles.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Supprimer date (OK, pas de données importantes)
            migrationBuilder.DropColumn(
                name: "date",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date",
                table: "Orders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 1, 14, 0, 0, 0, 0, DateTimeKind.Utc));
        }
    }
}
