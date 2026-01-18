using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billetterie_Spectacles.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorOrderAndTicketSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "price",
                table: "Tickets",
                newName: "unit_price");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "unit_price",
                table: "Tickets",
                newName: "price");
        }
    }
}
