using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billetterie_Spectacles.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentIntentIdToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "payment_intent_id",
                table: "Orders",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payment_intent_id",
                table: "Orders");
        }
    }
}
